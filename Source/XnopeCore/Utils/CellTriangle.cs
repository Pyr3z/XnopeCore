using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Xnope
{
    public struct CellTriangle : IEnumerable<IntVec3>
    {
        private IntVec3 a;
        private IntVec3 b;
        private IntVec3 c;
        private CellLine lineAB;
        private CellLine lineAC;
        private CellLine lineBC;
        private List<IntVec3> cellsInt;

        public IntVec3 A
        {
            get
            {
                return a;
            }
        }

        public IntVec3 B
        {
            get
            {
                return b;
            }
        }

        public IntVec3 C
        {
            get
            {
                return c;
            }
        }

        public IEnumerable<IntVec3> Cells
        {
            get
            {
                InitCellsList();

                foreach (var cell in cellsInt) yield return cell;
            }
        }

        public List<IntVec3> CellsList
        {
            get
            {
                InitCellsList();

                return cellsInt;
            }
        }

        public IEnumerable<IntVec3> EdgeAB
        {
            get
            {
                return a.CellsInLineTo(b);
            }
        }

        public IEnumerable<IntVec3> EdgeAC
        {
            get
            {
                return a.CellsInLineTo(c);
            }
        }

        public IEnumerable<IntVec3> EdgeBC
        {
            get
            {
                return b.CellsInLineTo(c);
            }
        }

        public IEnumerable<IntVec3> Edges
        {
            get
            {
                return EdgeAB.Concat(EdgeAC).Concat(EdgeBC);
            }
        }

        public IntVec3 RandomCell
        {
            get
            {
                return CellsList.RandomElement();
            }
        }

        public IntVec3 Centre
        {
            get
            {
                return a.AverageWith(b, c);
            }
        }

        public CellTriangle(IntVec3 a, IntVec3 b, IntVec3 c)
        {
            if (a == b || a == c || b == c)
            {
                throw new ArgumentException("[XnopeCore] Tried to construct a triangle with two equal cells. a=" + a + ", b=" + b + ", c=" + c);
            }

            this.a = a;
            this.b = b;
            this.c = c;

            if (b.IsClockwiseOfWRT(c, a))
            {
                this.b = c;
                this.c = b;
            }

            lineAB = CellLine.Between(a, b);
            lineAC = CellLine.Between(a, c);
            lineBC = CellLine.Between(b, c);

            cellsInt = null;
        }

        public static CellTriangle FromTarget(IntVec3 start, IntVec3 targ, float halfAngle, float height)
        {
            var sideLength = height / Mathf.Cos(halfAngle * Mathf.PI / 180f);
            var sideVec = (targ.ToVector3Shifted() - start.ToVector3Shifted()).normalized * sideLength;

            var b = sideVec.RotatedBy(halfAngle).ToIntVec3() + start;
            var c = sideVec.RotatedBy(-halfAngle).ToIntVec3() + start;

            return new CellTriangle(start, b, c);
        }

        public CellTriangle ClipInside(CellRect rect)
        {
            // Shift a

            if (a.x < rect.minX)
            {
                a.x = rect.minX;
            }
            else if (a.x > rect.maxX)
            {
                a.x = rect.maxX;
            }
            if (a.z < rect.minZ)
            {
                a.z = rect.minZ;
            }
            else if (a.z > rect.maxZ)
            {
                a.z = rect.maxZ;
            }

            // Shift b

            if (b.x < rect.minX)
            {
                b.x = rect.minX;
            }
            else if (b.x > rect.maxX)
            {
                b.x = rect.maxX;
            }
            if (b.z < rect.minZ)
            {
                b.z = rect.minZ;
            }
            else if (b.z > rect.maxZ)
            {
                b.z = rect.maxZ;
            }

            // Shift c

            if (c.x < rect.minX)
            {
                c.x = rect.minX;
            }
            else if (c.x > rect.maxX)
            {
                c.x = rect.maxX;
            }
            if (c.z < rect.minZ)
            {
                c.z = rect.minZ;
            }
            else if (c.z > rect.maxZ)
            {
                c.z = rect.maxZ;
            }

            lineAB = CellLine.Between(a, b);
            lineAC = CellLine.Between(a, c);
            lineBC = CellLine.Between(b, c);

            if (!cellsInt.NullOrEmpty())
            {
                cellsInt.Clear();
            }

            return this;
        }

        public bool Contains(IntVec3 cell)
        {
            return CellsUtil.CellIsBetween(lineAB, lineAC, cell)
                && CellsUtil.CellIsBetween(lineBC, lineAB, cell);
        }

        public IEnumerable<IntVec3> RandomUniqueCells(int num, Predicate<IntVec3> validator = null)
        {
            var used = new HashSet<int>();
            var iRange = new IntRange(0, CellsList.Count - 1);

            IntVec3 cell;
            int index = iRange.RandomInRange;
            while (num > 0)
            {
                int i = 0;
                while (used.Contains(index) && i < CellsList.Count)
                {
                    index = iRange.RandomInRange;
                    i++;
                }

                if (i == CellsList.Count)
                {
                    Log.Error("[XnopeCore] Ran out of cells to return randomly from a triangle. a=" + a + ", b=" + b + ", c=" + c);
                    break;
                }

                used.Add(index);

                cell = CellsList[index];

                if (validator == null || validator(cell))
                {
                    yield return cell;
                    num--;
                }
            }
        }

        public bool TryRandomCell(out IntVec3 cell, Predicate<IntVec3> validator = null)
        {
            var tempCell = IntVec3.Invalid;

            for (int i = 0; i < CellsList.Count; i++)
            {
                if (!CellsList.TryRandomElement(out tempCell))
                {
                    cell = IntVec3.Invalid;
                    return false;
                }

                if (validator == null || validator(tempCell))
                {
                    cell = tempCell;
                    return true;
                }
            }

            cell = IntVec3.Invalid;
            return false;
        }


        private void InitCellsList()
        {
            if (cellsInt == null)
            {
                cellsInt = new List<IntVec3>();
            }

            if (!cellsInt.Any())
            {
                cellsInt.Clear();

                var candidates = CellRect.FromLimits(
                    new IntVec3(Mathf.Min(a.x, b.x, c.x), 0, Mathf.Min(a.z, b.z, c.z)),
                    new IntVec3(Mathf.Max(a.x, b.x, c.x), 0, Mathf.Max(a.z, b.z, c.z))
                );

                foreach (var cell in candidates)
                {
                    if (Contains(cell))
                    {
                        cellsInt.Add(cell);
                    }
                }
            }
        }

        public IEnumerator<IntVec3> GetEnumerator()
        {
            return Cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cells.GetEnumerator();
        }
    }
}

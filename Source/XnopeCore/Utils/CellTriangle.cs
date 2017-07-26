using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Xnope
{
    public struct CellTriangle : IEnumerable<IntVec3>
    {
        private IntVec3 a;
        private IntVec3 b;
        private IntVec3 c;
        private IntVec3 centreInt;
        private CellLine lineAB;
        private CellLine lineAC;
        private CellLine lineBC;
        private List<IntVec3> cellsInt;

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

        public IEnumerable<IntVec3> CellsInLineAB
        {
            get
            {
                return a.CellsInLineTo(b);
            }
        }

        public IEnumerable<IntVec3> CellsInLineAC
        {
            get
            {
                return a.CellsInLineTo(c);
            }
        }

        public IEnumerable<IntVec3> CellsInLineBC
        {
            get
            {
                return b.CellsInLineTo(c);
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
                if (!centreInt.IsValid)
                {
                    centreInt = a.AverageWith(b, c);
                }

                return centreInt;
            }
        }

        public CellTriangle(IntVec3 a, IntVec3 b, IntVec3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            lineAB = CellLine.Between(a, b);
            lineAC = CellLine.Between(a, c);
            lineBC = CellLine.Between(b, c);
            centreInt = IntVec3.Invalid;
            cellsInt = null;
        }

        public static CellTriangle FromTarget(IntVec3 start, IntVec3 targ, float halfAngle, float sideLength)
        {
            var dirVec = targ.ToVector3Shifted() - start.ToVector3Shifted();
            dirVec = Vector3.ClampMagnitude(dirVec, sideLength);

            var b = dirVec.RotatedBy(halfAngle).ToIntVec3() + start;
            var c = dirVec.RotatedBy(-halfAngle).ToIntVec3() + start;

            return new CellTriangle(start, b, c);
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

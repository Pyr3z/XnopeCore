using System;
using System.Collections.Generic;
using Verse;

namespace Xnope.Util
{
    public static class Cells
    {
        public static IntVec3 Average(this IEnumerable<IntVec3> vecs)
        {
            int totalX = 0;
            int totalZ = 0;
            int count = 0;

            foreach (IntVec3 vec in vecs)
            {
                totalX += vec.x;
                totalZ += vec.z;
                count++;
            }

            return new IntVec3(totalX / count, 0, totalZ / count);
        }

        public static IntVec3 Average(params IntVec3[] vecs)
        {
            return vecs.Average();
        }

        public static IEnumerable<IntVec3> CellsInLineTo(this IntVec3 a, IntVec3 b, bool debug = false)
        {
            // Holy shit tho. It works, it's efficient, and it took me sooo long to figure out.
            if (!a.InBounds(Find.VisibleMap) || !b.InBounds(Find.VisibleMap))
            {
                Log.Error("Cell out of map bounds. a=" + a + " b=" + b);
            }

            if (debug)
                Log.Warning("[Debug] (" + a.x + ", 0, " + a.z + ")");

            yield return a;

            int dx = b.x - a.x;
            int dz = b.z - a.z;

            int x = a.x;
            int z = a.z;

            int d;
            int r;

            int dxa = dx < 0 ? -dx : dx;
            int dza = dz < 0 ? -dz : dz;

            if (dxa > dza)
            {
                d = dxa / (dza + 1);
                r = dxa % (dza + 1);
            }
            else if (dxa < dza)
            {
                d = dza / (dxa + 1);
                r = dza % (dxa + 1);
            }
            else
            {
                d = dxa;
                r = 0;
            }

            while (dx != 0 || dz != 0)
            {
                // handle straight lines
                if (dx == 0 && dz != 0)
                {
                    if (dz > 0)
                    {
                        z++;
                        dz--;
                    }
                    else
                    {
                        z--;
                        dz++;
                    }

                    if (debug)
                        Log.Warning("[Debug] (" + x + ", 0, " + z + ")");

                    yield return new IntVec3(x, 0, z);
                }
                else if (dz == 0 && dx != 0)
                {
                    if (dx > 0)
                    {
                        x++;
                        dx--;
                    }
                    else
                    {
                        x--;
                        dx++;
                    }

                    if (debug)
                        Log.Warning("[Debug] (" + x + ", 0, " + z + ")");

                    yield return new IntVec3(x, 0, z);
                }
                else
                {
                    // non-straight lines
                    for (int i = 0; i < d; i++)
                    {
                        if (dx == -dz && dx != 0)
                        {
                            if (dx > dz)
                            {
                                x++;
                                z--;
                                dx--;
                                dz++;
                            }
                            else
                            {
                                x--;
                                z++;
                                dx++;
                                dz--;
                            }
                        }
                        else if (dx < dz)
                        {
                            if (dx > 0 || dza > dxa)
                            {
                                if (dz > 0)
                                {
                                    z++;
                                    dz--;
                                }
                                else
                                {
                                    z--;
                                    dz++;
                                }
                            }
                            else
                            {
                                x--;
                                dx++;
                            }
                        }
                        else if (dx > dz)
                        {
                            if (dz > 0 || dxa > dza)
                            {
                                if (dx > 0)
                                {
                                    x++;
                                    dx--;
                                }
                                else
                                {
                                    x--;
                                    dx++;
                                }
                            }
                            else
                            {
                                z--;
                                dz++;
                            }
                        }
                        else if (dx == dz && dx != 0)
                        {
                            if (dx > 0)
                            {
                                x++;
                                z++;
                                dx--;
                                dz--;
                            }
                            else
                            {
                                x--;
                                z--;
                                dx++;
                                dz++;
                            }
                        }
                        else // dx == dz && dx == 0
                        {
                            break;
                        }

                        if (debug)
                            Log.Warning("[Debug] (" + x + ", 0, " + z + ")");

                        yield return new IntVec3(x, 0, z);
                    }

                    // handle increment
                    if (dx > dz && dz != 0)
                    {
                        if (dz > 0)
                        {
                            z++;
                            dz--;
                        }
                        else
                        {
                            z--;
                            dz++;
                        }

                        // handle remainder
                        if (r != 0)
                        {
                            if (dx > 0)
                            {
                                x++;
                                dx--;
                                r--;
                            }
                            else if (dx < 0)
                            {
                                x--;
                                dx++;
                                r--;
                            }
                        }

                        if (debug)
                            Log.Warning("[Debug] (" + x + ", 0, " + z + ")");

                        yield return new IntVec3(x, 0, z);
                    }
                    else if (dx < dz && dx != 0)
                    {
                        if (dx > 0)
                        {
                            x++;
                            dx--;
                        }
                        else
                        {
                            x--;
                            dx++;
                        }

                        // handle remainder
                        if (r != 0)
                        {
                            if (dz > 0)
                            {
                                z++;
                                dz--;
                                r--;
                            }
                            else if (dz < 0)
                            {
                                z--;
                                dz++;
                                r--;
                            }
                        }

                        if (debug)
                            Log.Warning("[Debug] (" + x + ", 0, " + z + ")");

                        yield return new IntVec3(x, 0, z);
                    }
                }
            } // end while
        }

        public static int CountMineableCellsTo(this IntVec3 from, IntVec3 to, Map map, bool consecutive = false)
        {
            int numMineable = 0;
            int numMineableConsecutive = 0;
            foreach (var cell in from.CellsInLineTo(to))
            {
                var obst = cell.GetCover(map);
                if (obst != null && obst.def.mineable)
                {
                    numMineable++;
                }
                else if (consecutive)
                {
                    if (numMineable > numMineableConsecutive)
                        numMineableConsecutive = numMineable;

                    numMineable = 0;
                }
            }

            return consecutive ? numMineableConsecutive : numMineable;
        }

        public static float DistanceSquaredToNearestMineable(this IntVec3 cell, Map map, int searchRadius, out IntVec3 mineable)
        {
            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, false))
            {
                var cover = cel.GetCover(map);
                if (cover != null && cover.def.mineable)
                {
                    mineable = cel;
                    return cell.DistanceToSquared(cel);
                }
            }

            mineable = IntVec3.Invalid;
            return float.MaxValue;
        }

        public static IntVec3 FurthestCellFrom(this CellRect rect, IntVec3 point, bool edgeCellsOnly = false, Predicate<IntVec3> validator = null)
        {
            IntVec3 result = rect.CenterCell;
            float distanceSquared = 0f;

            foreach (var cell in edgeCellsOnly ? rect.EdgeCells : rect.Cells)
            {
                if (validator == null || validator(cell))
                {
                    float tempDistanceSqrd = cell.DistanceToSquared(point);
                    if (tempDistanceSqrd > distanceSquared)
                    {
                        result = cell;
                        distanceSquared = tempDistanceSqrd;
                    }
                }
            }

            return result;
        }

        public static bool IsAroundTerrainOfTag(this IntVec3 spot, Map map, int radius, string tag)
        {
            foreach (var cell in GenRadial.RadialCellsAround(spot, radius, true))
            {
                if (!cell.InBounds(map)) continue;

                if (cell.GetTerrain(map).HasTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public static IntVec3 ToIntVec3(this Rot4 rot, byte shiftedBy = 0)
        {
            byte rotb = rot.AsByte;
            rotb += shiftedBy;
            rotb %= 4;

            switch (rotb)
            {
                case 0: // North
                    return IntVec3.North;

                case 1: // East
                    return IntVec3.East;

                case 2: // South
                    return IntVec3.South;

                case 3: // West
                    return IntVec3.West;

                default:
                    Log.Error("Error when converting Rot4 to IntVec3. Expect more errors.");
                    return IntVec3.Invalid;
            }
        }
    }
}
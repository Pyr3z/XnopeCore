using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Xnope
{
    /// <summary>
    /// Utilities for RimWorld 'cells'.
    /// <para />
    /// Cells are generally represented as IntVec3's (3-D vectors with integer dimensions),
    /// or sometimes as LocalTargetInfo's (a struct that is castable to/from an IntVec3 or Thing).
    /// </summary>
    public static class CellsUtil
    {
        /// <summary>
        /// Averages an IEnumerable of cells, with an optional multiplicity function that
        /// determines how much weight a specific kind of cell should have on the average.
        /// </summary>
        /// <param name="cells">IEnumerable of IntVec3</param>
        /// <param name="multiplicityFactorFunc">If not null, the result of this function
        /// is effectively the number of times the passed cell is counted in the average.</param>
        /// <returns></returns>
        public static IntVec3 Average(this IEnumerable<IntVec3> cells, Func<IntVec3, int> multiplicityFactorFunc = null)
        {
            int totalX = 0;
            int totalZ = 0;
            int count = 0;

            int multiplicity = 1;

            foreach (var cell in cells)
            {
                if (multiplicityFactorFunc != null)
                {
                    multiplicity = multiplicityFactorFunc(cell);
                }

                totalX += cell.x * multiplicity;
                totalZ += cell.z * multiplicity;
                count += multiplicity;
            }

            return count == 0 ? IntVec3.Invalid : new IntVec3(totalX / count, 0, totalZ / count);
        }

        /// <summary>
        /// Averages an IEnumerable of cells, with an optional multiplicity function that
        /// determines how much weight a specific kind of cell should have on the average.
        /// </summary>
        /// <param name="cells">IEnumerable of LocalTargetInfo</param>
        /// <param name="multiplicityFactorFunc">If not null, the result of this function
        /// is effectively the number of times the passed cell is counted in the average.</param>
        /// <returns></returns>
        public static IntVec3 Average(this IEnumerable<LocalTargetInfo> cells, Func<LocalTargetInfo, int> multiplicityFactorFunc = null)
        {
            int totalX = 0;
            int totalZ = 0;
            int count = 0;

            int multiplicity = 1;

            foreach (var cell in cells)
            {
                if (multiplicityFactorFunc != null)
                {
                    multiplicity = multiplicityFactorFunc(cell);
                }

                IntVec3 vec = cell.Cell;

                totalX += vec.x * multiplicity;
                totalZ += vec.z * multiplicity;
                count += multiplicity;
            }

            return count == 0 ? IntVec3.Invalid : new IntVec3(totalX / count, 0, totalZ / count);
        }

        public static IntVec3 Average(Func<IntVec3, int> multiplicityFactorFunc = null, params IntVec3[] cells)
        {
            return cells.Average(multiplicityFactorFunc);
        }

        /// <summary>
        /// Averages the passed cells, with an optional multiplicity function that
        /// determines how much weight a specific kind of cell should have on the average.
        /// </summary>
        /// <param name="multiplicityFactorFunc">If not null, the result of this function
        /// is effectively the number of times the passed cell is counted in the average.</param>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static IntVec3 Average(Func<LocalTargetInfo, int> multiplicityFactorFunc = null, params LocalTargetInfo[] cells)
        {
            return cells.Average(multiplicityFactorFunc);
        }

        /// <summary>
        /// Yields the cells in a line from a to b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="debug">If true, will print each cell in the line to the log window.</param>
        /// <returns></returns>
        public static IEnumerable<IntVec3> CellsInLineTo(this IntVec3 a, IntVec3 b, bool debug = false)
        {
            // Holy shit tho. It works, it's efficient, and it took me sooo long to figure out.
            if (Find.VisibleMap != null && (!a.InBounds(Find.VisibleMap) || !b.InBounds(Find.VisibleMap)))
            {
                Log.Error("Cell out of map bounds while calculating a line. Calculation will continue, but you may expect further errors. a=" + a + " b=" + b);
            }

            if (debug)
                Log.Message("[Debug] (" + a.x + ", 0, " + a.z + ")");

            yield return a;

            int x = a.x;
            int z = a.z;

            int dx = b.x - x; // the change in x to reach b.x
            int dz = b.z - z; // the change in z to reach b.z

            int dxa = dx < 0 ? -dx : dx; // absolute value of dx
            int dza = dz < 0 ? -dz : dz; // absolute value of dz

            int d; // how many distinct intermediate lines there should be between a and b
            int r; // remainder: used to compensate for indivisible differentials

            // avoid a value of d < 1:
            if (dxa > dza)
            {
                // line relative to x+ direction is less than 45-degree angle
                d = dxa / (dza + 1);
                r = dxa % (dza + 1);
            }
            else if (dxa < dza)
            {
                // line relative to x+ direction is greater than 45-degree angle
                d = dza / (dxa + 1);
                r = dza % (dxa + 1);
            }
            else
            {
                // line relative to x+ direction is a 45-degree angle
                d = dxa;
                r = 0;
            }

            // do calculations until we've reached b
            while (dx != 0 || dz != 0)
            {
                // EZ-PZ straight lines :)
                if (dx == 0 && dz != 0)
                {
                    if (dz > 0)
                    {
                        // go up
                        z++;
                        dz--;
                    }
                    else
                    {
                        // go down
                        z--;
                        dz++;
                    }

                    if (debug)
                        Log.Message("[Debug] (" + x + ", 0, " + z + ")");

                    yield return new IntVec3(x, 0, z);
                }
                else if (dz == 0 && dx != 0)
                {
                    if (dx > 0)
                    {
                        // go right
                        x++;
                        dx--;
                    }
                    else
                    {
                        // go left
                        x--;
                        dx++;
                    }

                    if (debug)
                        Log.Message("[Debug] (" + x + ", 0, " + z + ")");

                    yield return new IntVec3(x, 0, z);
                }
                else
                {
                    // non-straight lines : do intermediate lines
                    for (int i = 0; i < d; i++)
                    {
                        if (dx == dz && dx != 0)
                        {
                            // do diagonal (quadrants I & III)
                            if (dx > 0)
                            {
                                // right-up
                                x++;
                                z++;
                                dx--;
                                dz--;
                            }
                            else
                            {
                                // left-down
                                x--;
                                z--;
                                dx++;
                                dz++;
                            }
                        }
                        else if (dx == -dz && dx != 0)
                        {
                            // go diagonal (quadrants II & IV)
                            if (dx > dz)
                            {
                                // right-down
                                x++;
                                z--;
                                dx--;
                                dz++;
                            }
                            else
                            {
                                // left-up
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
                                // more dz to do than dx
                                if (dz > 0)
                                {
                                    // up
                                    z++;
                                    dz--;
                                }
                                else
                                {
                                    // down
                                    z--;
                                    dz++;
                                }
                            }
                            else // more dx to do than dz, and dx is negative
                            {
                                // left
                                x--;
                                dx++;
                            }
                        }
                        else if (dx > dz)
                        {
                            if (dz > 0 || dxa > dza)
                            {
                                // more dx to do than dz
                                if (dx > 0)
                                {
                                    // right
                                    x++;
                                    dx--;
                                }
                                else
                                {
                                    // left
                                    x--;
                                    dx++;
                                }
                            }
                            else // more dz to do than dx, and dz is negative
                            {
                                // down
                                z--;
                                dz++;
                            }
                        }
                        else // dx == 0 && dz == 0
                        {
                            // shouldn't do any more because we've reached b
                            break;
                        }

                        if (debug)
                            Log.Message("[Debug] (" + x + ", 0, " + z + ")");

                        yield return new IntVec3(x, 0, z);
                    } // end d for-loop

                    // increment next intermediate line
                    if (dx > dz && dz != 0)
                    {
                        if (dz > 0)
                        {
                            // adjacent shift up
                            z++;
                            dz--;
                        }
                        else
                        {
                            // adjacent shift down
                            z--;
                            dz++;
                        }

                        // remainder compensation
                        if (r != 0)
                        {
                            // if still some remainder, make it a diagonal line
                            // instead of directly adjacent
                            if (dx > 0)
                            {
                                // additional shift right
                                x++;
                                dx--;
                                r--;
                            }
                            else if (dx < 0)
                            {
                                // additional shift left
                                x--;
                                dx++;
                                r--;
                            }
                        }

                        if (debug)
                            Log.Message("[Debug] (" + x + ", 0, " + z + ")");

                        yield return new IntVec3(x, 0, z);
                    }
                    else if (dx < dz && dx != 0)
                    {
                        if (dx > 0)
                        {
                            // adjacent shift right
                            x++;
                            dx--;
                        }
                        else
                        {
                            // adjacent shift left
                            x--;
                            dx++;
                        }

                        // remainder compensation
                        if (r != 0)
                        {
                            // if still some remainder, make it a diagonal line
                            // instead of directly adjacent
                            if (dz > 0)
                            {
                                // additional shift up
                                z++;
                                dz--;
                                r--;
                            }
                            else if (dz < 0)
                            {
                                // additional shift down
                                z--;
                                dz++;
                                r--;
                            }
                        }

                        if (debug)
                            Log.Message("[Debug] (" + x + ", 0, " + z + ")");

                        yield return new IntVec3(x, 0, z);
                    }
                }
            } // end while
            // we did it
        }

        /// <summary>
        /// Yields the edges of a rect, sans corners.
        /// <para />
        /// Order: South-East-North-West.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static IEnumerable<IntVec3> CornerlessEdgeCells(this CellRect rect)
        {
            int x = rect.minX + 1;
            int z = rect.minZ;
            while (x < rect.maxX)
            {
                yield return new IntVec3(x, 0, z);
                x++;
            }
            for (z++; z < rect.maxZ; z++)
            {
                yield return new IntVec3(x, 0, z);
            }
            for (x--; x > rect.minX; x--)
            {
                yield return new IntVec3(x, 0, z);
            }
            for (z--; z > rect.minZ; z--)
            {
                yield return new IntVec3(x, 0, z);
            }
        }

        /// <summary>
        /// Counts the number of mineable cells between a and be.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="map"></param>
        /// <param name="consecutive">If true, returns the highest number of consecutive mineable cells.</param>
        /// <returns></returns>
        public static int CountMineableCellsTo(this IntVec3 a, IntVec3 b, Map map, bool consecutive = false)
        {
            if (!a.InBounds(map) || !b.InBounds(map))
            {
                Log.Error("One or both cells are not within the map: " + a + ", " + b);
                return 0;
            }

            int numMineable = 0;
            int numMineableConsecutive = 0;
            foreach (var cell in a.CellsInLineTo(b))
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

        /// <summary>
        /// Returns the square distance between cell and the nearest mineabl cell.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        public static float DistanceSquaredToNearestMineable(this IntVec3 cell, Map map, int searchRadius)
        {
            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                var cover = cel.GetCover(map);
                if (cover != null && cover.def.mineable)
                {
                    return cell.DistanceToSquared(cel);
                }
            }

            return float.MaxValue;
        }

        /// <summary>
        /// Returns the square distance between cell and the nearest mineable cell.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <param name="mineable">The closest mineable cell.</param>
        /// <returns></returns>
        public static float DistanceSquaredToNearestMineable(this IntVec3 cell, Map map, int searchRadius, out IntVec3 mineable)
        {
            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

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

        /// <summary>
        /// Returns the square distance between cell and the nearest mineable cell.
        /// <para />
        /// The mineable out parameter is assigned a LocalTargetInfo which wraps the Thing of the cell,
        /// not merely its IntVec3.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <param name="mineable">The closest mineable cell, with a Thing instead of an IntVec3.</param>
        /// <returns></returns>
        public static float DistanceSquaredToNearestMineable(this LocalTargetInfo cell, Map map, int searchRadius, out LocalTargetInfo mineable)
        {
            IntVec3 cellvec = cell.Cell;
            foreach (var cel in GenRadial.RadialCellsAround(cellvec, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                var cover = cel.GetCover(map);
                if (cover != null && cover.def.mineable)
                {
                    mineable = cover;
                    return cellvec.DistanceToSquared(cel);
                }
            }

            mineable = IntVec3.Invalid;
            return float.MaxValue;
        }

        /// <summary>
        /// Finds the furthest cell in the rect from point.
        /// <para />
        /// If there is no validator, only edge cells are checked,
        /// as logically only edge cells could ever be returned.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="point"></param>
        /// <param name="edgeCellsOnly">Forced to true if validator is null. Science, b*tch.</param>
        /// <param name="validator"></param>
        /// <returns></returns>
        public static IntVec3 FurthestCellFrom(this CellRect rect, LocalTargetInfo point, bool edgeCellsOnly = true, Predicate<IntVec3> validator = null)
        {
            IntVec3 result = rect.CenterCell;
            IntVec3 pointVec = point.Cell;
            float distanceSquared = 0f;

            if (validator == null && !edgeCellsOnly) edgeCellsOnly = true;

            foreach (var cell in edgeCellsOnly ? rect.EdgeCells : rect.Cells)
            {
                if (validator == null || validator(cell))
                {
                    float tempDistanceSqrd = cell.DistanceToSquared(pointVec);
                    if (tempDistanceSqrd > distanceSquared)
                    {
                        result = cell;
                        distanceSquared = tempDistanceSqrd;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the cell is around any terrain with the given tag, in the given search radius.
        /// <para />
        /// Example terrain tags would be "Water" or "Road".
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsAroundTerrainOfTag(this IntVec3 cell, Map map, int searchRadius, string tag)
        {
            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                if (cel.GetTerrain(map).HasTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the cell is around any terrain with the given tag, in the given search radius.
        /// <para />
        /// Example terrain tags would be "Water" or "Road".
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool IsAroundTerrainOfTag(this LocalTargetInfo cell, Map map, int searchRadius, string tag)
        {
            IntVec3 vec = cell.Cell;
            foreach (var cel in GenRadial.RadialCellsAround(vec, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                if (cel.GetTerrain(map).HasTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the nearest standable cell within the searchRadius, or IntVec3.Invalid if none exists.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        public static IntVec3 NearestStandableCell(this IntVec3 from, Map map, int searchRadius)
        {
            if (!from.InBounds(map))
            {
                Log.Error("Cell out of bounds: " + from);
                return IntVec3.Invalid;
            }

            foreach (var cell in GenRadial.RadialCellsAround(from, searchRadius, true))
            {
                if (cell.Standable(map))
                    return cell;
            }

            return IntVec3.Invalid;
        }

        /// <summary>
        /// Returns the cardinal direction facing 'to', from the perspective of 'from'.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Rot4 RotationFacing(this IntVec3 from, IntVec3 to)
        {
            var dir = to - from;

            if (dir.x == 0)
            {
                return dir.z < 0 ? Rot4.South : Rot4.North;
            }

            if (dir.z == 0)
            {
                return dir.x < 0 ? Rot4.West : Rot4.East;
            }

            if (dir.x > 0)
            {
                if (dir.z > 0)
                {
                    // Quadrant I
                    if (dir.x > dir.z)
                    {
                        return Rot4.East;
                    }
                    else
                    {
                        return Rot4.North;
                    }
                }
                else
                {
                    // Quadrant IV
                    if (dir.x > -dir.z)
                    {
                        return Rot4.East;
                    }
                    else
                    {
                        return Rot4.South;
                    }
                }
            }
            else
            {
                if (dir.z > 0)
                {
                    // Quadrant II
                    if (-dir.x > dir.z)
                    {
                        return Rot4.West;
                    }
                    else
                    {
                        return Rot4.North;
                    }
                }
                else
                {
                    // Quadrant III
                    if (dir.x < dir.z)
                    {
                        return Rot4.West;
                    }
                    else
                    {
                        return Rot4.South;
                    }
                }
            }

        }

        /// <summary>
        /// Returns the IntVec3 vector equivalent of the given rotation, optionally shifted clock-wise.
        /// <para />
        /// Example result: Rot4.East -> (1,0,0) || Rot4.West -> (-1,0,0)
        /// </summary>
        /// <param name="rot"></param>
        /// <param name="shiftedBy"></param>
        /// <returns></returns>
        public static IntVec3 ToIntVec3(this Rot4 rot, byte shiftedBy = 0)
        {
            return rot.AsByte.ToIntVec3(shiftedBy);
        }

        /// <summary>
        /// Returns the IntVec3 vector equivalent of the given rotation, optionally shifted clock-wise.
        /// </summary>
        /// <param name="rotb"></param>
        /// <param name="shiftedBy"></param>
        /// <returns></returns>
        public static IntVec3 ToIntVec3(this byte rotb, byte shiftedBy = 0)
        {
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

        public static bool TryFindNearestRoadCell(this IntVec3 cell, Map map, int searchRadius, out IntVec3 roadCell)
        {
            if (!map.roadInfo.roadEdgeTiles.Any())
            {
                roadCell = IntVec3.Invalid;
                return false;
            }

            var dist = float.MaxValue;
            var tempCell = IntVec3.Invalid;

            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                if (cel.GetTerrain(map).HasTag("Road"))
                {
                    var tempDist = cell.DistanceToSquared(cel);

                    if (tempDist < dist)
                    {
                        dist = tempDist;
                        tempCell = cel;
                    }
                }
            }

            roadCell = tempCell;

            return tempCell.IsValid;
        }

        /// <summary>
        /// If there are road cells on the edge of map, this will return the nearest one.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <param name="roadCell">Will be IntVec3.Invalid if return is false.</param>
        /// <returns></returns>
        public static bool TryFindNearestRoadEdgeCell(this IntVec3 cell, Map map, out IntVec3 roadCell)
        {
            var roadEdgeTiles = map.roadInfo.roadEdgeTiles;

            if (!roadEdgeTiles.Any())
            {
                roadCell = IntVec3.Invalid;
                return false;
            }

            var dist = float.MaxValue;
            var tempCell = IntVec3.Invalid;

            foreach (var rcell in roadEdgeTiles)
            {
                var tempDist = cell.DistanceToSquared(rcell);

                if (tempDist < dist)
                {
                    dist = tempDist;
                    tempCell = rcell;
                }
            }

            roadCell = tempCell;

            return tempCell.IsValid;
        }

        /// <summary>
        /// If there are road cells on the edge of the map, this returns the nearest collection of them.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IEnumerable<IntVec3> TryFindNearestRoadEdgeCells(this IntVec3 cell, Map map)
        {
            IntVec3 initialRoadCell;
            if (!TryFindNearestRoadEdgeCell(cell, map, out initialRoadCell))
            {
                yield break;
            }

            var roadEdgeCells = map.roadInfo.roadEdgeTiles;

            // just yield the edge cells around the initial nearest road cell found
            foreach (var rcell in GenRadial.RadialCellsAround(initialRoadCell, 3, true)
                                  .Where(c => roadEdgeCells.Contains(c)))
            {
                yield return rcell;
            }
        }

    }
}
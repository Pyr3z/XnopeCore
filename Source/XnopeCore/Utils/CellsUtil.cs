using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

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
        private static IntVec3 cachedAverageColPos = IntVec3.Invalid;

        public static void Cleanup()
        {
            cachedAverageColPos = IntVec3.Invalid;
        }


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

        public static IntVec3 AverageWith(this IntVec3 orig, params IntVec3[] others)
        {
            IntVec3[] arr = new IntVec3[others.Length + 1];

            arr[0] = orig;
            for (int i = 1; i < arr.Length; i++)
            {
                arr[i] = others[i - 1];
            }

            return arr.Average();
        }

        public static IntVec3 AverageColonistPosition(Map map, bool cache = true)
        {
            if (cache && cachedAverageColPos.IsValid)
            {
                return cachedAverageColPos;
            }

            var colonistThingsLocList = new List<LocalTargetInfo>();

            foreach (var pawn in map.mapPawns.FreeColonistsSpawned)
            {
                colonistThingsLocList.Add(pawn);
            }
            foreach (var building in map.listerBuildings.allBuildingsColonist.Where(b => b.def == ThingDefOf.Wall || b.def == ThingDefOf.Door || b is IAttackTarget))
            {
                colonistThingsLocList.Add(building);
            }

            var ave = colonistThingsLocList.Average(delegate (LocalTargetInfo c)
            {
                if (c.Thing is Pawn)
                {
                    return 2;
                }
                else if (c.Thing.def == ThingDefOf.Sandbags)
                {
                    return 5;
                }

                return 1;
            });

            Log.Message("[XnopeCore] Cached average colonist position: " + ave);
            cachedAverageColPos = ave;

            return ave;
        }

        public static IntVec3 ApproxClosestColonistBuilding(Map map, IntVec3 from, ThingDef def)
        {
            var result = IntVec3.Invalid;
            var ave = from.AverageWith(AverageColonistPosition(map));

            ave.TryFindNearestColonistBuilding(map, out result, def);

            return result;
        }

        /// <summary>
        /// Checks if c is between lineA and lineB.
        /// <para />
        /// lineA must be counter-clockwise of lineB w.r.t. their intersection, and they cannot be equal.
        /// </summary>
        /// <param name="lineA"></param>
        /// <param name="lineB"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool CellIsBetween(CellLine lineA, CellLine lineB, IntVec3 c)
        {
            bool aPos = lineA.Slope > 0;
            bool bPos = lineB.Slope > 0;
            bool oppositeSigns = aPos != bPos;
            bool sameQuad = !oppositeSigns && lineA.Slope > lineB.Slope;
            bool oppositeQuads = !oppositeSigns && lineA.Slope < lineB.Slope;

            if (sameQuad || (bPos && !aPos))
            {
                return lineA.CellIsLeft(c) == lineB.CellIsLeft(c);
            }

            if (oppositeQuads || (aPos && !bPos))
            {
                return lineA.CellIsLeft(c) != lineB.CellIsLeft(c);
            }

            // I may have figured this out on my own, but I still have no fucking clue why it works.

            return lineA.CellIsAbove(c) != lineB.CellIsAbove(c);
        }

        /// <summary>
        /// Similar to CellLine.CellIsAbove(), tells if a cell is left of a line. This accounts for whether or not a slope is negative.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool CellIsLeft(this CellLine line, IntVec3 c)
        {
            return c.x < (c.z - line.ZIntercept) / line.Slope;
        }

        /// <summary>
        /// Yields the cells in a line from a to b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="useOldVersion">If true, will use the old version of the calculation. (My version.)</param>
        /// <returns></returns>
        public static IEnumerable<IntVec3> CellsInLineTo(this IntVec3 a, IntVec3 b, bool useOldVersion = false)
        {
            if (Find.VisibleMap != null && (!a.InBounds(Find.VisibleMap) || !b.InBounds(Find.VisibleMap)))
            {
                Log.Error("[XnopeCore] Cell out of map bounds while calculating a line. Calculation will continue, but you may expect further errors. a=" + a + " b=" + b);
            }

            if (!useOldVersion)
            {
                // Copyright Verse.GenSight.PointsOnLineOfSight(). It's called the Bresenham Algorithm.
                bool sideOnEqual;
                if (a.x == b.x)
                {
                    sideOnEqual = (a.z < b.z);
                }
                else
                {
                    sideOnEqual = (a.x < b.x);
                }

                int dx = Mathf.Abs(b.x - a.x);
                int dz = Mathf.Abs(b.z - a.z);
                int x = a.x;
                int z = a.z;
                int i = 1 + dx + dz;
                int x_inc = (b.x <= a.x) ? -1 : 1;
                int z_inc = (b.z <= a.z) ? -1 : 1;
                int error = dx - dz;
                dx *= 2;
                dz *= 2;

                var cell = a;
                while (i > 1)
                {
                    cell.x = x;
                    cell.z = z;
                    yield /* fucking */ return /* the god damn */ cell;

                    if (error > 0 || (error == 0 && sideOnEqual))
                    {
                        x += x_inc;
                        error -= dz;
                    }
                    else
                    {
                        z += z_inc;
                        error += dx;
                    }
                    i--;
                }

                yield break;
            }

            yield return a;

            int x_stupid = a.x;
            int z_stupid = a.z;

            int dx_stupid = b.x - x_stupid; // the change in x to reach b.x
            int dz_stupid = b.z - z_stupid; // the change in z to reach b.z

            int dxa = dx_stupid < 0 ? -dx_stupid : dx_stupid; // absolute value of dx
            int dza = dz_stupid < 0 ? -dz_stupid : dz_stupid; // absolute value of dz

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
            while (dx_stupid != 0 || dz_stupid != 0)
            {
                // EZ-PZ straight lines :)
                if (dx_stupid == 0 && dz_stupid != 0)
                {
                    if (dz_stupid > 0)
                    {
                        // go up
                        z_stupid++;
                        dz_stupid--;
                    }
                    else
                    {
                        // go down
                        z_stupid--;
                        dz_stupid++;
                    }

                    yield return new IntVec3(x_stupid, 0, z_stupid);
                }
                else if (dz_stupid == 0 && dx_stupid != 0)
                {
                    if (dx_stupid > 0)
                    {
                        // go right
                        x_stupid++;
                        dx_stupid--;
                    }
                    else
                    {
                        // go left
                        x_stupid--;
                        dx_stupid++;
                    }

                    yield return new IntVec3(x_stupid, 0, z_stupid);
                }
                else
                {
                    // non-straight lines : do intermediate lines
                    for (int i = 0; i < d; i++)
                    {
                        if (dx_stupid == dz_stupid && dx_stupid != 0)
                        {
                            // do diagonal (quadrants I & III)
                            if (dx_stupid > 0)
                            {
                                // right-up
                                x_stupid++;
                                z_stupid++;
                                dx_stupid--;
                                dz_stupid--;
                            }
                            else
                            {
                                // left-down
                                x_stupid--;
                                z_stupid--;
                                dx_stupid++;
                                dz_stupid++;
                            }
                        }
                        else if (dx_stupid == -dz_stupid && dx_stupid != 0)
                        {
                            // go diagonal (quadrants II & IV)
                            if (dx_stupid > dz_stupid)
                            {
                                // right-down
                                x_stupid++;
                                z_stupid--;
                                dx_stupid--;
                                dz_stupid++;
                            }
                            else
                            {
                                // left-up
                                x_stupid--;
                                z_stupid++;
                                dx_stupid++;
                                dz_stupid--;
                            }
                        }
                        else if (dx_stupid < dz_stupid)
                        {
                            if (dx_stupid > 0 || dza > dxa)
                            {
                                // more dz to do than dx
                                if (dz_stupid > 0)
                                {
                                    // up
                                    z_stupid++;
                                    dz_stupid--;
                                }
                                else
                                {
                                    // down
                                    z_stupid--;
                                    dz_stupid++;
                                }
                            }
                            else // more dx to do than dz, and dx is negative
                            {
                                // left
                                x_stupid--;
                                dx_stupid++;
                            }
                        }
                        else if (dx_stupid > dz_stupid)
                        {
                            if (dz_stupid > 0 || dxa > dza)
                            {
                                // more dx to do than dz
                                if (dx_stupid > 0)
                                {
                                    // right
                                    x_stupid++;
                                    dx_stupid--;
                                }
                                else
                                {
                                    // left
                                    x_stupid--;
                                    dx_stupid++;
                                }
                            }
                            else // more dz to do than dx, and dz is negative
                            {
                                // down
                                z_stupid--;
                                dz_stupid++;
                            }
                        }
                        else // dx == 0 && dz == 0
                        {
                            // shouldn't do any more because we've reached b
                            break;
                        }

                        yield return new IntVec3(x_stupid, 0, z_stupid);
                    } // end d for-loop

                    // increment next intermediate line
                    if (dx_stupid > dz_stupid && dz_stupid != 0)
                    {
                        if (dz_stupid > 0)
                        {
                            // adjacent shift up
                            z_stupid++;
                            dz_stupid--;
                        }
                        else
                        {
                            // adjacent shift down
                            z_stupid--;
                            dz_stupid++;
                        }

                        // remainder compensation
                        if (r != 0)
                        {
                            // if still some remainder, make it a diagonal line
                            // instead of directly adjacent
                            if (dx_stupid > 0)
                            {
                                // additional shift right
                                x_stupid++;
                                dx_stupid--;
                                r--;
                            }
                            else if (dx_stupid < 0)
                            {
                                // additional shift left
                                x_stupid--;
                                dx_stupid++;
                                r--;
                            }
                        }

                        yield return new IntVec3(x_stupid, 0, z_stupid);
                    }
                    else if (dx_stupid < dz_stupid && dx_stupid != 0)
                    {
                        if (dx_stupid > 0)
                        {
                            // adjacent shift right
                            x_stupid++;
                            dx_stupid--;
                        }
                        else
                        {
                            // adjacent shift left
                            x_stupid--;
                            dx_stupid++;
                        }

                        // remainder compensation
                        if (r != 0)
                        {
                            // if still some remainder, make it a diagonal line
                            // instead of directly adjacent
                            if (dz_stupid > 0)
                            {
                                // additional shift up
                                z_stupid++;
                                dz_stupid--;
                                r--;
                            }
                            else if (dz_stupid < 0)
                            {
                                // additional shift down
                                z_stupid--;
                                dz_stupid++;
                                r--;
                            }
                        }

                        yield return new IntVec3(x_stupid, 0, z_stupid);
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
                else if (consecutive && numMineable > numMineableConsecutive)
                {
                    numMineableConsecutive = numMineable;

                    numMineable = 0;
                }
            }

            return consecutive ? numMineableConsecutive : numMineable;
        }

        public static int CountObstructingCellsTo(this IntVec3 a, IntVec3 b, Map map, bool consecutive = false)
        {
            if (!a.InBounds(map) || !b.InBounds(map))
            {
                Log.Error("[XnopeCore] Error counting obstructions. One or both cells are not within the map: " + a + ", " + b);
                return 0;
            }

            int num = 0;
            int numConsecutive = 0;
            foreach (var cell in a.CellsInLineTo(b))
            {
                if (!cell.CanBeSeenOverFast(map))
                {
                    num++;
                }
                else if (consecutive && num > numConsecutive)
                {
                    numConsecutive = num;

                    num = 0;
                }
            }

            return consecutive ? numConsecutive : num;
        }

        public static int CountObstructingCells(this IEnumerable<IntVec3> cells, Map map)
        {
            int num = 0;

            foreach (var cell in cells)
            {
                if (!cell.InBounds(map)) continue;

                if (!cell.CanBeSeenOverFast(map))
                {
                    num++;
                }
            }

            return num;
        }

        public static IntVec3 ClosestCellTo(this IEnumerable<IntVec3> cells, IntVec3 b, Map map)
        {
            var dist = float.MaxValue;
            var result = IntVec3.Invalid;

            foreach (var cell in cells)
            {
                if (!cell.InBounds(map)) continue;

                var tempDist = cell.DistanceToSquared(b);
                if (tempDist < dist)
                {
                    dist = tempDist;
                    result = cell;
                }
            }

            return result;
        }

        public static float DistanceSquaredToNearestColonyBuilding(this IntVec3 cell, Map map, ThingDef ofDef = null, bool requireLineOfSight = false)
        {
            float dist = float.MaxValue;

            if (!cell.InBounds(map))
            {
                Log.Error("[XnopeCore] Tried to get square distance to nearest colony building from " + cell + ", but it is out of bounds.");
                return dist;
            }

            IEnumerable<Building> colonyBuildings;

            if (ofDef == null)
            {
                colonyBuildings = map.listerBuildings.allBuildingsColonist;
            }
            else
            {
                colonyBuildings = map.listerBuildings.AllBuildingsColonistOfDef(ofDef);
            }

            foreach (var pos in colonyBuildings.Select(b => b.Position))
            {
                if (!requireLineOfSight || GenSight.LineOfSight(cell, pos, map, true))
                {
                    float tempDist = cell.DistanceToSquared(pos);
                    if (tempDist < dist)
                    {
                        dist = tempDist;
                    }
                }
            }

            return dist;
        }

        /// <summary>
        /// Returns the square distance between cell and the nearest mineabl cell.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        public static float DistanceSquaredToNearestMineable(this IntVec3 cell, Map map, float searchRadius)
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
        public static float DistanceSquaredToNearestMineable(this IntVec3 cell, Map map, float searchRadius, out IntVec3 mineable)
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
        public static float DistanceSquaredToNearestMineable(this LocalTargetInfo cell, Map map, float searchRadius, out LocalTargetInfo mineable)
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

        public static float DistanceSquaredToNearestRoad(this IntVec3 cell, Map map, float searchRadius)
        {
            if (!map.roadInfo.roadEdgeTiles.Any())
            {
                return float.MaxValue;
            }

            var dist = float.MaxValue;

            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                if (cel.GetTerrain(map).HasTag("Road"))
                {
                    var tempDist = cell.DistanceToSquared(cel);

                    if (tempDist < dist)
                    {
                        dist = tempDist;
                    }
                }
            }

            return dist;
        }

        public static float DistanceToMapEdge(this IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map))
            {
                Log.Error("[XnopeCore] Tried to get distance from " + cell + " to map edge, but it is out of bounds.");
                return float.MaxValue;
            }

            var centre = map.Center;
            var size = map.Size;

            if (cell.x < centre.x)
            {
                if (cell.z < centre.z)
                {
                    return Mathf.Min(cell.x, cell.z);
                }
                else
                {
                    return Mathf.Min(cell.x, size.z - cell.z);
                }
            }
            else
            {
                if (cell.z < centre.z)
                {
                    return Mathf.Min(size.x - cell.x, cell.z);
                }
                else
                {
                    return Mathf.Min(size.x - cell.x, size.z - cell.z);
                }
            }


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

        public static bool IsAroundGoodTerrain(this IntVec3 cell, Map map, float searchRadius)
        {
            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                var terr = cel.GetTerrain(map);

                if (cel.Roofed(map) || terr.affordances.NullOrEmpty() || terr.avoidWander || terr.HasTag("Water"))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAroundTerrainAffordances(this IntVec3 cell, Map map, float searchRadius, params TerrainAffordance[] affordances)
        {
            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                if (cel.GetTerrain(map).affordances.ContainsAll(affordances))
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
        public static bool IsAroundTerrainOfTag(this IntVec3 cell, Map map, float searchRadius, string tag)
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
        public static bool IsAroundTerrainOfTag(this LocalTargetInfo cell, Map map, float searchRadius, string tag)
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
        /// Returns if a is clockwise of b in the X-Z plane, with respect to wrt.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="wrt"></param>
        /// <returns></returns>
        public static bool IsClockwiseOfWRT(this IntVec3 a, IntVec3 b, IntVec3 wrt)
        {
            // Via cross product for the Y-value
            var vecA = a - wrt;
            var vecB = b - wrt;

            return vecA.z * vecB.x - vecA.x * vecB.z < 0;
        }

        /// <summary>
        /// Returns the nearest standable cell within the searchRadius, or IntVec3.Invalid if none exists.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="map"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        public static IntVec3 NearestStandableCell(this IntVec3 from, Map map, float searchRadius)
        {
            if (!from.InBounds(map))
            {
                Log.Warning("[XnopeCore] Tried to find nearest standable cell, but it is out of bounds: " + from);
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

        public static IEnumerable<IntVec3> RandomTriangularBisections(IntVec3 a, IntVec3 dir, float halfAngle, float sideLength, float minDist = 0f, int numBisections = 1)
        {
            var lineAB = TriangleSide(a, dir, halfAngle, sideLength, minDist);
            var lineAC = TriangleSide(a, dir, -halfAngle, sideLength, minDist);

            var maxI = Mathf.Min(lineAB.Length, lineAC.Length);

            int bisections = 0;
            while (bisections < numBisections)
            {
                var randI = Rand.RangeInclusive(0, maxI - 1);

                foreach (var cell in lineAB[randI].CellsInLineTo(lineAC[randI]))
                {
                    yield return cell;
                }

                bisections++;
            }
        }

        /// <summary>
        /// Yields the cells in the area of a right triangle.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="width"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        public static IEnumerable<IntVec3> RightTriangleArea(IntVec3 a, int width, bool up)
        {
            if (width == 0)
            {
                Log.Error("[XnopeCore] Tried to get an area of a right triangle, but width is 0. width=" + width);
                yield break;
            }

            var xOffset = IntVec3.East * width;
            var zOffset = IntVec3.North * (up ? width < 0 ? -width : width : width < 0 ? width : -width);
            var b = a + xOffset;
            var c = a + zOffset;

            var x_inc = width < 0 ? -1 : 1;
            var z_inc = up ? 1 : -1;

            // May be the one case where my shitty method is better than Tynan's:
            var hyp = b.CellsInLineTo(c, true).ToArray();

            var cell = a;
            yield return cell;

            int i = 0;
            while (i < hyp.Length)
            {
                while (cell.x != hyp[i].x)
                {
                    cell.x += x_inc;
                    yield return cell;
                }

                cell.x = a.x;
                
                if (cell.z != c.z)
                {
                    cell.z += z_inc;
                    yield return cell;
                }

                i++;
            }
        }

        public static IEnumerable<IntVec3> RightTriangleArea(IntVec3 a, int width, Rot4 rot1, Rot4 rot2)
        {
            if (rot1 == Rot4.North && rot2 == Rot4.East)
            {
                return RightTriangleArea(a, width < 0 ? -width : width, true);
            }
            if (rot1 == Rot4.North && rot2 == Rot4.West)
            {
                return RightTriangleArea(a, width < 0 ? -width : width, true);
            }
            if (rot1 == Rot4.South && rot2 == Rot4.West)
            {
                return RightTriangleArea(a, width < 0 ? width : -width, false);
            }
            if (rot1 == Rot4.South && rot2 == Rot4.East)
            {
                return RightTriangleArea(a, width < 0 ? width : -width, false);
            }

            Log.Error("[XnopeCore] Tried to get a right triangle area with invalid rotations. rot1=" + rot1 + ", rot2=" + rot2);
            return null;
        }

        public static IntVec3 TranslateToward(this IntVec3 cell, Rot4 dir, int amount, Map map = null)
        {
            var result = cell + dir.ToIntVec3() * amount;

            if (map != null && !result.InBounds(map))
            {
                Log.Error("[XnopeCore] Tried to translate " + cell + " toward " + dir + " by " + amount + ", which resulted in a cell out of bounds. Expect further errors.");
            }

            return result;
        }

        public static IntVec3 TranslateToward(this IntVec3 cell, Rot4 dir, Predicate<IntVec3> until)
        {
            var inc = dir.ToIntVec3();
            for (int i = 0; i < 250; i++)
            {
                if (until(cell))
                {
                    return cell;
                }

                cell += inc;
            }

            Log.Error("[XnopeCore] Tried to translate a cell by predicate, which was never satisfied after 250 tries. cell=" + cell + ", dir=" + dir);
            return IntVec3.Invalid;
        }

        public static IntVec3 TranslateToward(this IntVec3 cell, IntVec3 targ, int amount, Map map = null)
        {
            foreach (var cel in cell.CellsInLineTo(targ))
            {
                if (amount-- == 1)
                {
                    if (map != null && !cel.InBounds(map))
                        Log.Error("[XnopeCore] Tried to translate " + cell + " toward " + targ + " by " + amount + ", which resulted in a cell out of bounds. Expect further errors.");

                    return cel;
                }
            }

            Log.Error("[XnopeCore] Tried to translate a cell by amount=" + amount + ", which was longer than the distance between " + cell + " and " + targ + ".");
            return IntVec3.Invalid;
        }

        public static IntVec3 TranslateToward(this IntVec3 cell, IntVec3 targ, Predicate<IntVec3> until)
        {
            foreach (var cel in cell.CellsInLineTo(targ))
            {
                if (until(cel))
                    return cel;
            }

            Log.Error("[XnopeCore] Tried to translate a cell by predicate, which was never satisfied. cell=" + cell + ", targ=" + targ);
            return IntVec3.Invalid;
        }

        public static IntVec3[] TriangleSide(IntVec3 a, IntVec3 dir, float angle, float sideLength, float minDist = 0f)
        {
            var dirVec = dir.ToVector3Shifted() - a.ToVector3Shifted();
            dirVec = Vector3.ClampMagnitude(dirVec, sideLength);

            var b = dirVec.RotatedBy(angle).ToIntVec3() + a;

            var bStart = a;

            if (minDist > 0f)
            {
                var minOffsetVec = Vector3.ClampMagnitude(dirVec, minDist);
                bStart = minOffsetVec.RotatedBy(angle).ToIntVec3() + a;
            }

            return bStart.CellsInLineTo(b).ToArray();
        }

        public static bool TryFindNearestColonistBuilding(this IntVec3 cell, Map map, out IntVec3 buildingCell, ThingDef ofDef = null)
        {
            buildingCell = IntVec3.Invalid;
            var dist = float.MaxValue;

            if (!cell.InBounds(map))
            {
                Log.Error("[XnopeCore] Tried to get square distance to nearest colony building from " + cell + ", but it is out of bounds.");
            }

            IEnumerable<Building> colonyBuildings;

            if (ofDef == null)
            {
                colonyBuildings = map.listerBuildings.allBuildingsColonist;
            }
            else
            {
                colonyBuildings = map.listerBuildings.AllBuildingsColonistOfDef(ofDef);
            }

            foreach (var pos in colonyBuildings.Select(b => b.Position))
            {
                float tempDist = cell.DistanceToSquared(pos);
                if (tempDist < dist)
                {
                    dist = tempDist;
                    buildingCell = pos;
                }
            }

            return buildingCell.IsValid;
        }

        public static bool TryFindNearestAndFurthestRoadCell(this IntVec3 cell, Map map, float searchRadius, out IntVec3 roadNear, out IntVec3 roadFar)
        {
            if (!map.roadInfo.roadEdgeTiles.Any())
            {
                roadNear = IntVec3.Invalid;
                roadFar = IntVec3.Invalid;
                return false;
            }

            var distNear = float.MaxValue;
            var distFar = 0;
            var nearCell = IntVec3.Invalid;
            var farCell = IntVec3.Invalid;

            foreach (var cel in GenRadial.RadialCellsAround(cell, searchRadius, true))
            {
                if (!cel.InBounds(map)) continue;

                if (cel.GetTerrain(map).HasTag("Road"))
                {
                    var tempDist = cell.DistanceToSquared(cel);

                    if (tempDist < distNear)
                    {
                        distNear = tempDist;
                        nearCell = cel;
                    }
                    else if (tempDist > distFar)
                    {
                        distFar = tempDist;
                        farCell = cel;
                    }
                }
            }

            roadNear = nearCell;
            roadFar = farCell;
            return roadNear.IsValid || roadFar.IsValid;
        }

        public static bool TryFindNearestRoadCell(this IntVec3 cell, Map map, float searchRadius, out IntVec3 roadCell)
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
            return roadCell.IsValid;
        }

        public static bool TryFindNearestCellToRoad(this IEnumerable<IntVec3> cells, Map map, float searchRadius, out IntVec3 nearCell)
        {
            if (!map.roadInfo.roadEdgeTiles.Any())
            {
                nearCell = IntVec3.Invalid;
                return false;
            }

            var dist = float.MaxValue;
            var tempCell = IntVec3.Invalid;

            var centre = cells.Average();
            IntVec3 dirRoad;

            if (TryFindNearestRoadCell(centre, map, searchRadius, out dirRoad))
            {
                foreach (var cell in cells)
                {
                    float tempDist = cell.DistanceToSquared(dirRoad);

                    if (tempDist < dist)
                    {
                        dist = tempDist;
                        tempCell = cell;
                    }
                }
            }

            nearCell = tempCell;

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
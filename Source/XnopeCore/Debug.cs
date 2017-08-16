using RimWorld;
using Verse;

namespace Xnope
{
    internal static class Debug
    {
        public static void TestLineDraw16Way(Map map, IntVec3 centre, int radius)
        {
            IntVec3[] endpoints = new IntVec3[]
            {
                new IntVec3(centre.x, 0, centre.z + radius),
                new IntVec3(centre.x, 0, centre.z - radius),
                new IntVec3(centre.x + radius, 0, centre.z),
                new IntVec3(centre.x - radius, 0, centre.z),

                new IntVec3(centre.x + radius, 0, centre.z + radius),
                new IntVec3(centre.x - radius, 0, centre.z + radius),
                new IntVec3(centre.x + radius, 0, centre.z - radius),
                new IntVec3(centre.x - radius, 0, centre.z - radius),

                new IntVec3(centre.x + radius, 0, centre.z + radius / 2),
                new IntVec3(centre.x - radius, 0, centre.z + radius / 2),
                new IntVec3(centre.x + radius, 0, centre.z - radius / 2),
                new IntVec3(centre.x - radius, 0, centre.z - radius / 2),

                new IntVec3(centre.x + radius / 2, 0, centre.z + radius),
                new IntVec3(centre.x - radius / 2, 0, centre.z + radius),
                new IntVec3(centre.x + radius / 2, 0, centre.z - radius),
                new IntVec3(centre.x - radius / 2, 0, centre.z - radius),
            };

            foreach (var point in endpoints)
            {
                TestLineDraw(map, centre, point);
            }
        }

        public static void TestLineDrawCardinal(Map map, IntVec3 centre, int radius)
        {
            IntVec3[] endpoints = new IntVec3[]
            {
                new IntVec3(centre.x, 0, centre.z + radius),
                new IntVec3(centre.x, 0, centre.z - radius),
                new IntVec3(centre.x + radius, 0, centre.z),
                new IntVec3(centre.x - radius, 0, centre.z),
            };

            foreach (var point in endpoints)
            {
                TestLineDraw(map, centre, point);
            }
        }

        public static void TestLineDrawDiagonal(Map map, IntVec3 centre, int radius)
        {
            IntVec3[] endpoints = new IntVec3[]
            {
                new IntVec3(centre.x + radius, 0, centre.z + radius),
                new IntVec3(centre.x - radius, 0, centre.z + radius),
                new IntVec3(centre.x + radius, 0, centre.z - radius),
                new IntVec3(centre.x - radius, 0, centre.z - radius),
            };

            foreach (var point in endpoints)
            {
                TestLineDraw(map, centre, point);
            }
        }

        public static void TestLineDraw8Way(Map map, IntVec3 centre, int radius)
        {
            IntVec3[] endpoints = new IntVec3[]
            {
                new IntVec3(centre.x, 0, centre.z + radius),
                new IntVec3(centre.x, 0, centre.z - radius),
                new IntVec3(centre.x + radius, 0, centre.z),
                new IntVec3(centre.x - radius, 0, centre.z),

                new IntVec3(centre.x + radius, 0, centre.z + radius),
                new IntVec3(centre.x - radius, 0, centre.z + radius),
                new IntVec3(centre.x + radius, 0, centre.z - radius),
                new IntVec3(centre.x - radius, 0, centre.z - radius),
            };

            foreach (var point in endpoints)
            {
                TestLineDraw(map, centre, point);
            }
        }

        public static void TestLineDraw12Way(Map map, IntVec3 centre, int radius)
        {
            IntVec3[] endpoints = new IntVec3[]
            {
                new IntVec3(centre.x, 0, centre.z + radius),
                new IntVec3(centre.x, 0, centre.z - radius),
                new IntVec3(centre.x + radius, 0, centre.z),
                new IntVec3(centre.x - radius, 0, centre.z),

                new IntVec3(centre.x + radius, 0, centre.z + radius / 2),
                new IntVec3(centre.x - radius, 0, centre.z + radius / 2),
                new IntVec3(centre.x + radius, 0, centre.z - radius / 2),
                new IntVec3(centre.x - radius, 0, centre.z - radius / 2),

                new IntVec3(centre.x + radius / 2, 0, centre.z + radius),
                new IntVec3(centre.x - radius / 2, 0, centre.z + radius),
                new IntVec3(centre.x + radius / 2, 0, centre.z - radius),
                new IntVec3(centre.x - radius / 2, 0, centre.z - radius),
            };

            foreach (var point in endpoints)
            {
                TestLineDraw(map, centre, point);
            }
        }

        public static void TestLineDraw(Map map, IntVec3 a, IntVec3 b)
        {
            Log.Warning("[XnopeCore] Spawning torches between " + a + " and " + b);

            foreach (var cell in a.CellsInLineTo(b, true))
            {
                if (!cell.GetThingList(map).Any(t => t.def == ThingDefOf.TorchLamp))
                {
                    var thing = ThingMaker.MakeThing(ThingDefOf.TorchLamp);

                    GenSpawn.Spawn(thing, cell, map);
                }
            }

            Log.Warning("[XnopeCore] End line between " + a + " and " + b);
        }

        public static void TestRightTriangleDraw(Map map)
        {
            Log.Warning("[XnopeCore] Spawning torches in right triangles.");

            var quadI = map.Center + IntVec3.North + IntVec3.East;
            var quadII = map.Center + IntVec3.North + IntVec3.West;
            var quadIII = map.Center + IntVec3.South + IntVec3.West;
            var quadIV = map.Center + IntVec3.South + IntVec3.East;

            foreach (var cell in CellsUtil.RightTriangleArea(quadI, 10, true))
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.TorchLamp);

                GenSpawn.Spawn(thing, cell, map);
            }

            foreach (var cell in CellsUtil.RightTriangleArea(quadII, -10, true))
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.TorchLamp);

                GenSpawn.Spawn(thing, cell, map);
            }

            foreach (var cell in CellsUtil.RightTriangleArea(quadIII, -10, false))
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.TorchLamp);

                GenSpawn.Spawn(thing, cell, map);
            }

            foreach (var cell in CellsUtil.RightTriangleArea(quadIV, 10, false))
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.TorchLamp);

                GenSpawn.Spawn(thing, cell, map);
            }
        }

        public static void TestCellTriangleDraw(Map map)
        {
            var centre = map.Center;
            var targ = CellRect.WholeMap(map).RandomCell;
            var halfAngle = Rand.Range(20, 80);
            var magn = 40;

            var tri = CellTriangle.FromTarget(centre, targ, halfAngle, magn).ClipInside(CellRect.WholeMap(map));

            Log.Warning("[XnopeCore] Debug CellTriangle: A=" + tri.A + ", B=" + tri.B + ", C=" + tri.C + ", SlopeAB=" + tri.LineAB.Slope + ", ZInterceptAB=" + tri.LineAB.ZIntercept + ", SlopeAC=" + tri.LineAC.Slope + ", ZInterceptAC=" + tri.LineAC.ZIntercept +  ", SlopeBC=" + tri.LineBC.Slope + ", ZInterceptBC=" + tri.LineBC.ZIntercept);

            tri.DebugFlashDraw();
        }

        public static void TestCellTriangleDraw45Case(Map map)
        {
            var centre = map.Center;
            var magn = 40;
            var halfAngle = 15f;
            var targ = IntVec3.Invalid;

            for (int i = 0; i < 8; i++)
            {
                switch (i)
                {
                    case 0:
                        targ = centre + IntVec3.North + IntVec3.East;
                        break;
                    case 1:
                        targ = centre + IntVec3.North;
                        break;
                    case 2:
                        targ = centre + IntVec3.North + IntVec3.West;
                        break;
                    case 3:
                        targ = centre + IntVec3.South + IntVec3.East;
                        break;
                    case 4:
                        targ = centre + IntVec3.South;
                        break;
                    case 5:
                        targ = centre + IntVec3.South + IntVec3.West;
                        break;
                    case 6:
                        targ = centre + IntVec3.East;
                        break;
                    case 7:
                        targ = centre + IntVec3.West;
                        break;
                }

                var tri = CellTriangle.FromTarget(centre, targ, halfAngle, magn).ClipInside(CellRect.WholeMap(map));

                Log.Warning("[XnopeCore] Debug CellTriangle: A=" + tri.A + ", B=" + tri.B + ", C=" + tri.C + ", SlopeAB=" + tri.LineAB.Slope + ", ZInterceptAB=" + tri.LineAB.ZIntercept + ", SlopeAC=" + tri.LineAC.Slope + ", ZInterceptAC=" + tri.LineAC.ZIntercept + ", SlopeBC=" + tri.LineBC.Slope + ", ZInterceptBC=" + tri.LineBC.ZIntercept);

                tri.DebugFlashDraw(1f / Rand.Range(0f, 1.0f));
            }

        }

        public static void TestCellTriangleDraw90Case(Map map)
        {
            var centre = map.Center;
            var magn = 40;
            var halfAngle = 55f;
            var targ = IntVec3.Invalid;

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        targ = centre + IntVec3.North + IntVec3.East;
                        break;
                    case 1:
                        targ = centre + IntVec3.North + IntVec3.West;
                        break;
                    case 2:
                        targ = centre + IntVec3.South + IntVec3.West;
                        break;
                    case 3:
                        targ = centre + IntVec3.South + IntVec3.East;
                        break;
                }

                var tri = CellTriangle.FromTarget(centre, targ, halfAngle, magn).ClipInside(CellRect.WholeMap(map));

                Log.Warning("[XnopeCore] Debug CellTriangle: A=" + tri.A + ", B=" + tri.B + ", C=" + tri.C + ", SlopeAB=" + tri.LineAB.Slope + ", ZInterceptAB=" + tri.LineAB.ZIntercept + ", SlopeAC=" + tri.LineAC.Slope + ", ZInterceptAC=" + tri.LineAC.ZIntercept + ", SlopeBC=" + tri.LineBC.Slope + ", ZInterceptBC=" + tri.LineBC.ZIntercept);

                tri.DebugFlashDraw(1f / Rand.Range(0f, 1.0f));
            }

        }
    }
}

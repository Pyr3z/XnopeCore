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
            var magn = 20;

            var tri = CellTriangle.FromTarget(centre, targ, halfAngle, magn);

            foreach (var cell in tri)
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.Snowman);

                GenSpawn.Spawn(thing, cell, map);
            }
        }
    }
}

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
            Log.Warning("[Debug] Spawning torches between " + a + " and " + b);

            foreach (var cell in a.CellsInLineTo(b, true))
            {
                if (!cell.GetThingList(map).Any(t => t.def == ThingDefOf.TorchLamp))
                {
                    var thing = ThingMaker.MakeThing(ThingDefOf.TorchLamp);

                    GenSpawn.Spawn(thing, cell, map);
                }
            }

            Log.Warning("[Debug] End line between " + a + " and " + b);
        }

    }
}

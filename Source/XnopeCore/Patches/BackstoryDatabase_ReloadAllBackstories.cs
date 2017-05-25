using Harmony;
using RimWorld;
using System.Linq;
using Verse;
using Xnope.Defs;

namespace Xnope.Patches
{
    [HarmonyPatch(typeof(BackstoryDatabase), "ReloadAllBackstories")]
    public static class BackstoryDatabase_ReloadAllBackstories
    {
        // Postfix patch:
        // injects changes to vanilla backstories
        static void Postfix()
        {
            Log.Message("Patch called");
            InjectBackstoryData();

        }


        private static void InjectBackstoryData()
        {
            foreach (var injector in DefDatabase<SpawnCategoryInjectorDef>.AllDefs)
            {
                foreach (var targetBS in injector.injectToBackstories)
                {
                    foreach (var bs in (from b in BackstoryDatabase.allBackstories.Values
                                        where b.Title.Equals(targetBS)
                                        select b))
                    {
                        bs.spawnCategories.Add(injector.newCategory);
                        if (Prefs.DevMode)
                            Log.Message("Added spawn category \'" + injector.newCategory + "\' to backstory \'" + targetBS + "\'");
                    }
                }
            }
        }
    }
}

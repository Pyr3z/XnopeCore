using Harmony;
using HugsLib;
using System.Reflection;
using Verse;
using RimWorld;
using Xnope.Defs;
using System.Linq;

namespace Xnope
{
    [StaticConstructorOnStartup]
    public class XnopeCoreMod : ModBase
    {
        public override string ModIdentifier { get { return "XnopeCore"; } }

        //protected override bool HarmonyAutoPatch { get { return false; } }

        static XnopeCoreMod()
        {

        }


        public override void DefsLoaded()
        {
            InjectSpawnCategories();
        }


        private static void InjectSpawnCategories()
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

using Harmony;
using HugsLib;
using System.Reflection;
using Verse;
using RimWorld;
using Xnope.Defs;

namespace Xnope
{
    [StaticConstructorOnStartup]
    public class XnopeCoreMod : ModBase
    {
        public override string ModIdentifier { get { return "XnopeCore"; } }

        protected override bool HarmonyAutoPatch { get { return false; } }

        static XnopeCoreMod()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.github.xnope.core");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // For some reason, ReloadAllBackstories() stopped being called
            // every DoPlayLoad(). I have no idea why, it was working fine
            // before. Thus, the following three lines are necessary.
            BackstoryDatabase.Clear();
            BackstoryDatabase.ReloadAllBackstories();
            BackstoryDef.ReloadModdedBackstories();
        }
    }
}

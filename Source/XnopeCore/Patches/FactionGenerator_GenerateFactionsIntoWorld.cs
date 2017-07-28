using Harmony;
using RimWorld;
using System.Linq;
using Verse;

namespace Xnope.Patches
{
#pragma warning disable CS1591
    [HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld")]
    public static class FactionGenerator_GenerateFactionsIntoWorld
    {
        // Prefix patch:
        // hides roaming factions
        static void Prefix()
        {
            HideRoamingFactions(true);

        }

        // Postfix patch:
        // unhides roaming factions
        static void Postfix()
        {
            HideRoamingFactions(false);

        }



        private static void HideRoamingFactions(bool hide)
        {
            foreach (FactionDef def in (from d in DefDatabase<FactionDef>.AllDefs
                                        where d.IsRoaming()
                                        select d))
            {
                def.hidden = hide;
            }
        }
    }
#pragma warning restore CS1591
}

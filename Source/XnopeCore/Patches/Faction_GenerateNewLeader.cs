using Harmony;
using RimWorld;
using System.Linq;
using Verse;

namespace Xnope.Patches
{
    [HarmonyPatch(typeof(Faction), "GenerateNewLeader")]
    public static class Faction_GenerateNewLeader
    {
        // Prefix patch:
        // stores the old (dead) leader's name for use in postfix
        static void Prefix(Faction __instance, out string __state)
        {
            __state = "";
            if (__instance.IsDynamicallyNamed())
            {
                // Store old leader's name
                if (__instance.leader != null)
                    __state = __instance.leader.NameStringShort;
                else
                    __state = "LNAME";

            }
        }

        // Postfix patch:
        // dynamically adjusts a faction's name with its leader's name
        static void Postfix(Faction __instance, string __state)
        {
            if (__instance.IsDynamicallyNamed())
            {
                // Resolve name with new leader name
                ResolveFactionName(__instance, __state);

            }

        }



        private static void RegenerateFactionName(Faction fac)
        {
            // Unused for now
            fac.Name = NameGenerator.GenerateName(
                fac.def.factionNameMaker,
                from f in Find.FactionManager.AllFactions select f.Name,
                false
            );
        }

        private static void ResolveFactionName(Faction f, string oldLeaderName)
        {
            f.Name = f.Name.Replace(oldLeaderName, f.leader.NameStringShort);
        }

        private static string GetAdjectiveForLeader(string leaderName, string adjectiveType)
        {
            // TODO
            return "";
        }
    }
}

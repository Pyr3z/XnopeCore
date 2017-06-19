using Harmony;
using RimWorld;
using System.Linq;
using Verse;

namespace Xnope.Patches
{
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "SetBackstoryInSlot")]
    public static class Patch_PawnBioAndNameGenerator_SetBackstoryInSlot
    {
        // Prefix patch:
        // selects a backstory based on a pawn's kind rather than faction,
        // and only failing that does it select based on faction;
        // failing THAT, defaults.
        static bool Prefix(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
        {
            if ((from kvp in BackstoryDatabase.allBackstories
                  where kvp.Value.shuffleable
                       && kvp.Value.spawnCategories.Contains(pawn.kindDef.backstoryCategory) // changed
                       && kvp.Value.slot == slot
                       && (slot != BackstorySlot.Adulthood || !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))
                  select kvp.Value).TryRandomElement(out backstory))
            {
                // Found backstory from PawnKindDef, cancelling original function.
                return false;
            }
            // Defaulting to original function.
            return true;
        }
    }
}

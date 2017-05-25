using Verse;

namespace Xnope
{
    // Does this seriously not exist in the game's code,
    // yet CompColorable exists?
    public class CompProperties_Colorable : CompProperties
    {

        public CompProperties_Colorable()
        {
            this.compClass = typeof(CompColorable);
        }
    }
}

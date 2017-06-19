using UnityEngine;
using Verse;

namespace Xnope
{
    // Does this seriously not exist in the game's code,
    // yet CompColorable exists?
    // I suppose you could just use colors in GraphicData
    public class CompProperties_Colorable : CompProperties
    {
        Color color;

        public CompProperties_Colorable()
        {
            this.compClass = typeof(CompColorable);
        }
    }
}

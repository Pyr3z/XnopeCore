using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Xnope.Defs
{
    public class SpawnCategoryInjectorDef : Def
    {
        public string newCategory;
        public bool ignoreChildhoods = false;
        public bool ignoreAdulthoods = false;
        public List<string> injectToBackstories = new List<string>();
        public List<string> injectToCategories = new List<string>();

        // Actually injected in XnopeCoreMod.DefsLoaded()
    }
}

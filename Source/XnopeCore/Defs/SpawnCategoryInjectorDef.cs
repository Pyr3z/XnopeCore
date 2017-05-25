using System.Collections.Generic;
using Verse;

namespace Xnope.Defs
{
    public class SpawnCategoryInjectorDef : Def
    {
        #region XML Data
        public string newCategory;
        public List<string> injectToBackstories = new List<string>();
        #endregion

        // Actually injected in Patch_BackstoryDatabase_ReloadAllBackstories.
    }
}

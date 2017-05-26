using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Xnope.Defs
{
    public class BackstoryDef : Def    // revised from CCL
    {
        #region XML Data
        public string baseDescription;
        public BodyType bodyTypeGlobal = BodyType.Undefined;
        public BodyType bodyTypeMale = BodyType.Male;
        public BodyType bodyTypeFemale = BodyType.Female;
        public string title;
        public string titleShort;
        public BackstorySlot slot = BackstorySlot.Adulthood;
        public bool shuffleable = true;
        public bool addToDatabase = true;
        public List<WorkTags> workAllows = new List<WorkTags>();
        public List<WorkTags> workDisables = new List<WorkTags>();
        public List<WorkTags> requiredWorkTags = new List<WorkTags>();
        public List<BackstoryDefListItem> skillGains = new List<BackstoryDefListItem>();
        public List<string> spawnCategories = new List<string>();
        public List<BackstoryDefListItem> forcedTraits = new List<BackstoryDefListItem>();
        public List<BackstoryDefListItem> disallowedTraits = new List<BackstoryDefListItem>();

        public int multiplicity = 1;
        #endregion

        public static BackstoryDef Named(string defName)
        {
            return DefDatabase<BackstoryDef>.GetNamed(defName);
        }

        public static void ReloadModdedBackstories()
        {
            foreach (Def def in DefDatabase<BackstoryDef>.AllDefs)
            {
                def.ResolveReferences();
            }
        }

        public override void ResolveReferences()
        {
            if (multiplicity < 1)
            {
                Log.Warning("Config error in " + defName + ": multiplicity must be >= 1");
            }

            // TODO: This is fairly inefficient code, but it works.
            for (int i = 0; i < multiplicity; i++)
                InternalResolveReferences(i);

            if (Prefs.DevMode)
            {
                if (multiplicity > 1)
                    Log.Message("Added " + defName + " backstory with multiplicity " + multiplicity);
                else
                    Log.Message("Added " + defName + " backstory");
            }
        }


        private bool InternalResolveReferences(int counter)
        {
            base.ResolveReferences();

            if (!this.addToDatabase) return false;

            if (BackstoryDatabase.allBackstories.ContainsKey(this.UniqueSaveKey(counter)))
            {
                Log.Warning(this.defName + " is duplicated. Skipping.");
                return false;
            }

            //if (BackstoryDatabase.allBackstories.Remove(defName)) { }

            Backstory b = new Backstory();

            if (!this.title.NullOrEmpty())
                b.SetTitle(this.title);
            else
            {
                Log.Error(this.defName + " requires a title in XML file. Skipping.");
                return false;
            }

            if (spawnCategories.NullOrEmpty())
            {
                Log.Error(this.defName + " requires a spawnCategory in XML file. Skipping.");
                return false;
            }
            else
                b.spawnCategories = spawnCategories;

            if (!titleShort.NullOrEmpty())
                b.SetTitleShort(titleShort);
            else
                b.SetTitleShort(b.Title);

            if (!baseDescription.NullOrEmpty())
                b.baseDesc = baseDescription;
            else
            {
                b.baseDesc = "Empty.";
            }

            b.bodyTypeGlobal = bodyTypeGlobal;
            b.bodyTypeMale = bodyTypeMale;
            b.bodyTypeFemale = bodyTypeFemale;

            b.slot = slot;

            b.shuffleable = shuffleable;



            if (workAllows.Count > 0)
            {
                foreach (WorkTags current in Enum.GetValues(typeof(WorkTags)))
                {
                    if (!workAllows.Contains(current))
                    {
                        b.workDisables |= current;
                    }
                }
            }
            else if (workDisables.Count > 0)
            {
                foreach (var tag in workDisables)
                {
                    b.workDisables |= tag;
                }
            }
            else
            {
                b.workDisables = WorkTags.None;
            }

            if (requiredWorkTags.Count > 0)
            {
                foreach (var tag in requiredWorkTags)
                {
                    b.requiredWorkTags |= tag;
                }
            }
            else
            {
                b.requiredWorkTags = WorkTags.None;
            }

            b.skillGains = skillGains.ToDictionary(i => i.defName, i => i.degree);

            if (forcedTraits.Count > 0)
            {
                b.forcedTraits = new List<TraitEntry>();

                foreach (var trait in forcedTraits)
                {
                    b.forcedTraits.Add(new TraitEntry(TraitDef.Named(trait.defName), trait.degree));
                }

            }

            if (disallowedTraits.Count > 0)
            {
                // This approach is better than Rainbeau Flambe's
                // Editable Backstories mod, as it allows multiple entries of
                // a spectrum trait to be disallowed.

                b.disallowedTraits = new List<TraitEntry>();

                foreach (var trait in disallowedTraits)
                {
                    b.disallowedTraits.Add(new TraitEntry(TraitDef.Named(trait.defName), trait.degree));
                }
            }

            b.ResolveReferences();
            b.PostLoad();
            b.identifier = this.UniqueSaveKey(counter);

            if (b.ConfigErrors(true).Any())
            {
                // Config errors, don't add
                return false;
            }

            BackstoryDatabase.AddBackstory(b);
            return true;
        }
    }

    public static class BackstoryDefExt
    {
        public static string UniqueSaveKey(this BackstoryDef def, int counter)
        {
            if (def.defName.StartsWith("XnopeBS_")) return def.defName + counter;
            return "XnopeBS_" + def.defName + counter;
        }
    }

    public struct BackstoryDefListItem
    {
        public string defName;
        public int degree;
    }
}

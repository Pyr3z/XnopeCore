using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Xnope.Defs
{
#pragma warning disable CS1591
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
                Log.Warning("[XnopeCore] Config error in " + defName + ": multiplicity must be >= 1");
            }

            bool flag= false;

            // TODO: This is fairly inefficient code, but it works.
            for (int i = 0; i < multiplicity; i++)
                if (InternalResolveReferences(i))
                    flag = true;

            if (flag && Prefs.DevMode)
            {
                if (multiplicity > 1)
                    Log.Message("[XnopeCore] Added " + defName + " backstory with multiplicity " + multiplicity);
                else
                    Log.Message("[XnopeCore] Added " + defName + " backstory");
            }
        }


        private bool InternalResolveReferences(int counter)
        {
            base.ResolveReferences();

            if (!this.addToDatabase) return false;

            if (BackstoryDatabase.allBackstories.ContainsKey(this.UniqueSaveKey(counter)))
            {
                Log.Warning("[XnopeCore] " + this.defName + " is duplicated. Skipping.");
                return false;
            }

            //if (BackstoryDatabase.allBackstories.Remove(defName)) { }

            Backstory b = new Backstory();

            if (!this.title.NullOrEmpty())
                b.SetTitle(this.title);
            else
            {
                Log.Error("[XnopeCore] " + this.defName + " requires a title in XML file. Skipping.");
                return false;
            }

            if (spawnCategories.NullOrEmpty())
            {
                Log.Error("[XnopeCore] " + this.defName + " requires a spawnCategory in XML file. Skipping.");
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

    public static class BackstoryUtilities
    {
        public static string UniqueSaveKey(this BackstoryDef def, int counter)
        {
            if (def.defName.StartsWith("XnopeBS_")) return def.defName + counter;
            return "XnopeBS_" + def.defName + counter;
        }

        public static Backstory Clone(this Backstory b)
        {
            Backstory b2 = new Backstory();

            b2.slot = b.slot;
            b2.SetTitle(b.Title);
            b2.SetTitleShort(b.TitleShort);
            b2.baseDesc = b.baseDesc;
            b2.skillGains = b.skillGains; // Not a deep copy
            b2.workDisables = b.workDisables; 
            b2.requiredWorkTags = b.requiredWorkTags;
            b2.spawnCategories = b.spawnCategories; // Not a deep copy
            b2.bodyTypeGlobal = b.bodyTypeGlobal;
            b2.bodyTypeFemale = b.bodyTypeFemale;
            b2.bodyTypeMale = b.bodyTypeMale;
            b2.forcedTraits = b.forcedTraits; // Not a deep copy
            b2.disallowedTraits = b.disallowedTraits; // Not a deep copy
            b2.shuffleable = b.shuffleable;

            b2.ResolveReferences();
            b2.PostLoad();
            int id = (int)ParseHelper.FromString(b.identifier.Last().ToString(), typeof(int));
            b2.identifier = b.identifier + (id + 1);

            return b2;
        }
    }

    public struct BackstoryDefListItem
    {
        public string defName;
        public int degree;
    }

#pragma warning restore CS1591
}

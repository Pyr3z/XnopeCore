using RimWorld;
using System.Xml;
using Verse;

namespace Xnope
{
    public class FactionExtension : DefModExtension
    {
        public FactionDef def;

        public bool isRoaming;
        public bool dynamicNaming;



        public void LoadDataFromXmlCustom(XmlNode root)
        {
            // Necessary?
            this.def = DefDatabase<FactionDef>.GetNamed(root.Name, true);
        }

    }



    public static class FactionExt
    {
        /* Extension Methods */

        public static bool IsRoaming(this Faction fac)
        {
            return IsRoaming(fac.def);
        }

        public static bool IsRoaming(this FactionDef def)
        {
            FactionExtension ext = def.GetModExtension<FactionExtension>();
            return ext != null ? ext.isRoaming : false;
        }



        public static bool IsDynamicallyNamed(this Faction fac)
        {
            return IsDynamicallyNamed(fac.def);
        }

        public static bool IsDynamicallyNamed(this FactionDef def)
        {
            FactionExtension ext = def.GetModExtension<FactionExtension>();
            return ext != null ? ext.dynamicNaming : false;
        }
    }
}

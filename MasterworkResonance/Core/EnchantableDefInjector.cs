using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class EnchantableDefInjector
    {
        public static void Inject()
        {
            int enchantmentAdded = 0;
            int biocodableAdded = 0;

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def == null)
                {
                    continue;
                }

                if (def.comps == null)
                {
                    def.comps = new List<CompProperties>();
                }

                if (CanReceiveEnchantComp(def) && !HasComp(def, typeof(CompEnchantments)))
                {
                    def.comps.Add(new CompProperties_Enchantments());
                    enchantmentAdded++;
                }

                if (CanReceiveBiocodableComp(def) && !HasComp(def, typeof(CompBiocodable)))
                {
                    def.comps.Add(new CompProperties_Biocodable());
                    biocodableAdded++;
                }
            }

            MasterworkResonanceConfig.LogMessage("[MasterworkResonance] Added resonance comp to " + enchantmentAdded +
                                                 " ThingDefs.");
            MasterworkResonanceConfig.LogMessage("[MasterworkResonance] Added biocodable comp to " + biocodableAdded +
                                                 " weapon ThingDefs.");
        }

        private static bool CanReceiveEnchantComp(ThingDef def)
        {
            if (!def.HasComp(typeof(CompQuality)))
            {
                return false;
            }

            return def.IsWeapon || def.IsApparel;
        }

        private static bool CanReceiveBiocodableComp(ThingDef def)
        {
            if (!def.HasComp(typeof(CompQuality)))
            {
                return false;
            }

            return def.IsWeapon;
        }

        private static bool HasComp(ThingDef def, System.Type compClass)
        {
            if (def == null || def.comps == null || compClass == null)
            {
                return false;
            }

            for (int i = 0; i < def.comps.Count; i++)
            {
                CompProperties props = def.comps[i];
                if (props != null && props.compClass == compClass)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
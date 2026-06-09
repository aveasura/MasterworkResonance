using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class EnchantableDefInjector
    {
        public static void Inject()
        {
            int added = 0;

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def == null)
                {
                    continue;
                }

                if (!CanReceiveEnchantComp(def))
                {
                    continue;
                }

                if (def.comps == null)
                {
                    def.comps = new List<CompProperties>();
                }

                bool alreadyHasComp = false;
                for (int i = 0; i < def.comps.Count; i++)
                {
                    if (def.comps[i] != null && def.comps[i].compClass == typeof(CompEnchantments))
                    {
                        alreadyHasComp = true;
                        break;
                    }
                }

                if (alreadyHasComp)
                {
                    continue;
                }

                def.comps.Add(new CompProperties_Enchantments());
                added++;
            }

            MasterworkResonanceConfig.LogMessage("[MasterworkResonance] Added resonance comp to " + added +
                                                 " ThingDefs.");
        }

        private static bool CanReceiveEnchantComp(ThingDef def)
        {
            if (!def.HasComp(typeof(CompQuality)))
            {
                return false;
            }

            return def.IsWeapon || def.IsApparel;
        }
    }
}
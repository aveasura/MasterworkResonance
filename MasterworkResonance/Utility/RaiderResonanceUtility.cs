using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class RaiderResonanceUtility
    {
        public static void TryApplyResonanceForPawn(Pawn pawn, PawnGenerationRequest request)
        {
            MasterworkResonanceSettings settings = MasterworkResonanceMod.Settings;
            if (settings == null || !settings.enableRaiderResonance)
            {
                return;
            }

            if (!RaiderGearQualityUtility.IsEligibleRaider(pawn, request))
            {
                return;
            }

            int slotIndex = 0;

            if (pawn.equipment != null)
            {
                List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
                if (equipment != null)
                {
                    for (int i = 0; i < equipment.Count; i++)
                    {
                        TryApplyResonance(equipment[i], pawn, request, slotIndex++);
                    }
                }
            }

            if (pawn.apparel != null && pawn.apparel.WornApparel != null)
            {
                List<Apparel> apparel = pawn.apparel.WornApparel;
                for (int i = 0; i < apparel.Count; i++)
                {
                    TryApplyResonance(apparel[i], pawn, request, slotIndex++);
                }
            }
        }

        private static void TryApplyResonance(ThingWithComps thing, Pawn pawn, PawnGenerationRequest request,
            int slotIndex)
        {
            if (thing == null || thing.def == null || !RaiderGearQualityUtility.IsEligibleGear(thing.def))
            {
                return;
            }

            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality == null)
            {
                return;
            }

            QualityCategory quality = compQuality.Quality;
            if (quality != QualityCategory.Masterwork && quality != QualityCategory.Legendary)
            {
                return;
            }

            CompEnchantments compEnchantments = thing.TryGetComp<CompEnchantments>();
            if (compEnchantments == null || compEnchantments.HasEnchantment)
            {
                return;
            }

            MasterworkResonanceSettings settings = MasterworkResonanceMod.Settings;
            if (settings == null)
            {
                return;
            }

            float chance = Clamp01(settings.GetAwakeningChance(quality) * settings.raiderResonanceChanceMultiplier);
            if (chance <= 0f)
            {
                return;
            }

            int seed = BuildSeed(pawn, request, thing, slotIndex);
            if (chance < 1f && ResonanceDeterministicRandom.StableRandom01(seed) >= chance)
            {
                return;
            }

            compEnchantments.TryGenerateForcedDeterministic(null, false,
                ResonanceDeterministicRandom.Combine(seed, 9001));
        }

        private static int BuildSeed(Pawn pawn, PawnGenerationRequest request, Thing thing, int slotIndex)
        {
            unchecked
            {
                int seed = ResonanceDeterministicRandom.StableStringHash("MasterworkResonance.RaiderResonance");
                seed = ResonanceDeterministicRandom.Combine(seed, slotIndex);

                if (pawn != null)
                {
                    seed = ResonanceDeterministicRandom.Combine(seed, pawn.thingIDNumber);

                    if (pawn.kindDef != null)
                    {
                        seed = ResonanceDeterministicRandom.Combine(seed,
                            ResonanceDeterministicRandom.StableStringHash(pawn.kindDef.defName));
                        seed = ResonanceDeterministicRandom.Combine(seed, pawn.kindDef.shortHash);
                    }

                    if (pawn.Map != null)
                    {
                        seed = ResonanceDeterministicRandom.Combine(seed, pawn.Map.uniqueID);
                    }
                }

                if (request.KindDef != null)
                {
                    seed = ResonanceDeterministicRandom.Combine(seed,
                        ResonanceDeterministicRandom.StableStringHash(request.KindDef.defName));
                    seed = ResonanceDeterministicRandom.Combine(seed, request.KindDef.shortHash);
                }

                if (thing != null)
                {
                    seed = ResonanceDeterministicRandom.Combine(seed, thing.thingIDNumber);

                    if (thing.def != null)
                    {
                        seed = ResonanceDeterministicRandom.Combine(seed,
                            ResonanceDeterministicRandom.StableStringHash(thing.def.defName));
                        seed = ResonanceDeterministicRandom.Combine(seed, thing.def.shortHash);
                    }

                    CompQuality compQuality = thing.TryGetComp<CompQuality>();
                    if (compQuality != null)
                    {
                        seed = ResonanceDeterministicRandom.Combine(seed, (int)compQuality.Quality);
                    }
                }

                return seed;
            }
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }
    }
}
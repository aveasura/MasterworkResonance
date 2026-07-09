using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class RaiderBiocodeUtility
    {
        public static void TryBiocodeResonantWeaponsForPawn(Pawn pawn, PawnGenerationRequest request)
        {
            if (RaidEvolutionCompatibilityUtility.ShouldSuppressMwRaiderFeatures)
            {
                return;
            }

            MasterworkResonanceSettings settings = MasterworkResonanceMod.Settings;
            if (settings == null || !settings.enableRaiderBiocode)
            {
                return;
            }

            if (!RaiderGearQualityUtility.IsEligibleRaider(pawn, request))
            {
                return;
            }

            if (pawn.equipment == null)
            {
                return;
            }

            List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
            if (equipment == null)
            {
                return;
            }

            for (int i = 0; i < equipment.Count; i++)
            {
                TryBiocodeWeapon(equipment[i], pawn, request, i);
            }
        }

        private static void TryBiocodeWeapon(ThingWithComps thing, Pawn pawn, PawnGenerationRequest request,
            int slotIndex)
        {
            if (thing == null || thing.def == null || !IsEligibleWeapon(thing.def))
            {
                return;
            }

            CompEnchantments compEnchantments = thing.TryGetComp<CompEnchantments>();
            if (compEnchantments == null || !compEnchantments.HasEnchantment)
            {
                return;
            }

            CompBiocodable compBiocodable = thing.TryGetComp<CompBiocodable>();
            if (compBiocodable == null || IsAlreadyBiocoded(compBiocodable))
            {
                return;
            }

            MasterworkResonanceSettings settings = MasterworkResonanceMod.Settings;
            if (settings == null)
            {
                return;
            }

            float chance = Clamp01(settings.raiderBiocodeChance);
            if (chance <= 0f)
            {
                return;
            }

            int seed = BuildSeed(pawn, request, thing, compEnchantments, slotIndex);
            if (chance < 1f && ResonanceDeterministicRandom.StableRandom01(seed) >= chance)
            {
                return;
            }

            TrySetBiocodedFor(compBiocodable, pawn, thing);
        }

        private static bool IsEligibleWeapon(ThingDef def)
        {
            return def != null && (def.IsMeleeWeapon || def.IsRangedWeapon);
        }

        private static bool IsAlreadyBiocoded(CompBiocodable compBiocodable)
        {
            if (compBiocodable == null)
            {
                return false;
            }

            try
            {
                FieldInfo codedPawnField = AccessTools.Field(typeof(CompBiocodable), "codedPawn");
                if (codedPawnField == null)
                {
                    return true;
                }

                return codedPawnField.GetValue(compBiocodable) != null;
            }
            catch
            {
                return true;
            }
        }

        private static void TrySetBiocodedFor(CompBiocodable compBiocodable, Pawn pawn, Thing thing)
        {
            if (compBiocodable == null || pawn == null)
            {
                return;
            }

            try
            {
                MethodInfo method = AccessTools.Method(typeof(CompBiocodable), "Biocode", new Type[] { typeof(Pawn) })
                                    ?? AccessTools.Method(typeof(CompBiocodable), "CodeFor",
                                        new Type[] { typeof(Pawn) })
                                    ?? AccessTools.Method(typeof(CompBiocodable), "BiocodeFor",
                                        new Type[] { typeof(Pawn) })
                                    ?? AccessTools.Method(typeof(CompBiocodable), "SetBiocodedFor",
                                        new Type[] { typeof(Pawn) });

                if (method != null)
                {
                    method.Invoke(compBiocodable, new object[] { pawn });
                    return;
                }

                FieldInfo codedPawnField = AccessTools.Field(typeof(CompBiocodable), "codedPawn");
                if (codedPawnField != null)
                {
                    codedPawnField.SetValue(compBiocodable, pawn);
                    return;
                }

                MasterworkResonanceConfig.LogWarning(
                    "[MasterworkResonance] Failed to biocode raider weapon: CompBiocodable API was not found for " +
                    (thing != null ? thing.LabelCap.ToString() : "unknown weapon") + ".");
            }
            catch (Exception ex)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] Failed to biocode raider weapon: " +
                                                     ex.Message);
            }
        }

        private static int BuildSeed(Pawn pawn, PawnGenerationRequest request, Thing thing,
            CompEnchantments compEnchantments, int slotIndex)
        {
            unchecked
            {
                int seed = ResonanceDeterministicRandom.StableStringHash("MasterworkResonance.RaiderBiocode");
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
                }

                if (compEnchantments != null)
                {
                    seed = ResonanceDeterministicRandom.Combine(seed,
                        ResonanceDeterministicRandom.StableStringHash(compEnchantments.EnchantmentId));
                    seed = ResonanceDeterministicRandom.Combine(seed, compEnchantments.Tier);
                    seed = ResonanceDeterministicRandom.Combine(seed, (int)(compEnchantments.Value * 100000f));
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
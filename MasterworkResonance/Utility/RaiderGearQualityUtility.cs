using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class RaiderGearQualityUtility
    {
        public static void TryUpgradeGearForPawn(Pawn pawn, PawnGenerationRequest request)
        {
            MasterworkResonanceSettings settings = MasterworkResonanceMod.Settings;
            if (settings == null || !settings.enableRaiderGearQuality)
            {
                return;
            }

            if (!IsEligibleRaider(pawn, request))
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
                        TryUpgradeThing(equipment[i], pawn, request, slotIndex++);
                    }
                }
            }

            if (pawn.apparel != null && pawn.apparel.WornApparel != null)
            {
                List<Apparel> apparel = pawn.apparel.WornApparel;
                for (int i = 0; i < apparel.Count; i++)
                {
                    TryUpgradeThing(apparel[i], pawn, request, slotIndex++);
                }
            }
        }

        public static bool IsEligibleRaider(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn == null || pawn.Destroyed)
            {
                return false;
            }

            if (request.Context != PawnGenerationContext.NonPlayer)
            {
                return false;
            }

            Faction faction = pawn.Faction;
            if (faction == null)
            {
                faction = request.Faction;
            }

            if (faction == null || faction.IsPlayer)
            {
                return false;
            }

            if (faction.PlayerRelationKind != FactionRelationKind.Hostile)
            {
                return false;
            }

            if (pawn.kindDef != null && !pawn.kindDef.isFighter)
            {
                return false;
            }

            return true;
        }

        private static void TryUpgradeThing(ThingWithComps thing, Pawn pawn, PawnGenerationRequest request,
            int slotIndex)
        {
            if (thing == null || thing.def == null || !IsEligibleGear(thing.def))
            {
                return;
            }

            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality == null)
            {
                return;
            }

            if (compQuality.Quality >= QualityCategory.Masterwork)
            {
                return;
            }

            MasterworkResonanceSettings settings = MasterworkResonanceMod.Settings;
            if (settings == null)
            {
                return;
            }

            float upgradeChance = Clamp01(settings.raiderGearQualityUpgradeChance);
            if (upgradeChance <= 0f)
            {
                return;
            }

            int seed = BuildSeed(pawn, request, thing, slotIndex);
            if (ResonanceDeterministicRandom.StableRandom01(seed) >= upgradeChance)
            {
                return;
            }

            float legendaryChance = Clamp01(settings.raiderGearLegendaryChance);
            bool legendary = legendaryChance > 0f &&
                             ResonanceDeterministicRandom.StableRandom01(
                                 ResonanceDeterministicRandom.Combine(seed, 7331)) < legendaryChance;

            QualityCategory newQuality = legendary ? QualityCategory.Legendary : QualityCategory.Masterwork;
            compQuality.SetQuality(newQuality, ArtGenerationContext.Outsider);
        }

        public static bool IsEligibleGear(ThingDef def)
        {
            return def.IsMeleeWeapon || def.IsRangedWeapon || def.IsApparel;
        }

        private static int BuildSeed(Pawn pawn, PawnGenerationRequest request, Thing thing, int slotIndex)
        {
            unchecked
            {
                int seed = ResonanceDeterministicRandom.StableStringHash("MasterworkResonance.RaiderGearQuality");
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
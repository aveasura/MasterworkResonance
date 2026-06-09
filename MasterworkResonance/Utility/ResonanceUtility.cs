using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class ResonanceUtility
    {
        private static readonly string[] RelevantThingStatNames =
        {
            "MeleeWeapon_DamageMultiplier",
            "MeleeWeapon_CooldownMultiplier",
            "RangedWeapon_DamageMultiplier",
            "RangedWeapon_Cooldown",
            "ArmorRating_Sharp",
            "ArmorRating_Blunt",
            "ArmorRating_Heat",
            "MaxHitPoints",
            "AimingDelayFactor",
            "IncomingDamageFactor",
            "Thing_Weapon_MeleeWarmupTime",
            "MeleeHitChance",
            "MeleeDodgeChance",
            "ShootingAccuracyPawn"
        };

        private static readonly string[] RelevantPawnStatNames =
        {
            "MoveSpeed",
            "AimingDelayFactor",
            "IncomingDamageFactor",
            "MeleeHitChance",
            "MeleeDodgeChance",
            "ShootingAccuracyPawn",
            "Thing_Weapon_MeleeWarmupTime"
        };

        public static CompEnchantments GetCompFromThing(Thing thing)
        {
            if (thing == null)
            {
                return null;
            }

            MinifiedThing minifiedThing = thing as MinifiedThing;
            if (minifiedThing != null)
            {
                thing = minifiedThing.InnerThing;
            }

            ThingWithComps thingWithComps = thing as ThingWithComps;
            if (thingWithComps == null)
            {
                return null;
            }

            return thingWithComps.TryGetComp<CompEnchantments>();
        }

        public static bool TryGetThingEnchantValue(StatRequest req, string enchantmentId, out float value)
        {
            value = 0f;

            Thing thing = req.Thing;
            CompEnchantments comp = GetCompFromThing(thing);
            if (comp == null)
            {
                return false;
            }

            return comp.TryGetValue(enchantmentId, out value);
        }

        public static float GetWornApparelEnchantTotal(Pawn pawn, string enchantmentId, float cap)
        {
            if (pawn == null || pawn.apparel == null || pawn.apparel.WornApparel == null)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
            {
                Apparel apparel = pawn.apparel.WornApparel[i];
                CompEnchantments comp = GetCompFromThing(apparel);
                if (comp == null)
                {
                    continue;
                }

                float value;
                if (comp.TryGetValue(enchantmentId, out value))
                {
                    total += value;
                }
            }

            if (cap > 0f && total > cap)
            {
                total = cap;
            }

            return total;
        }

        public static float GetEquippedWeaponEnchantValue(Pawn pawn, string enchantmentId)
        {
            if (pawn == null || pawn.equipment == null)
            {
                return 0f;
            }

            ThingWithComps primary = pawn.equipment.Primary;
            if (primary == null)
            {
                return 0f;
            }

            CompEnchantments comp = GetCompFromThing(primary);
            if (comp == null)
            {
                return 0f;
            }

            float value;
            return comp.TryGetValue(enchantmentId, out value) ? value : 0f;
        }

        public static float GetTotalMoodBonus(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return GetEquippedWeaponEnchantValue(pawn, "MeleeMood")
                   + GetEquippedWeaponEnchantValue(pawn, "RangedMood")
                   + GetWornApparelEnchantTotal(pawn, "ApparelMood", 0f);
        }

        public static float GetTotalMeleeDodgeBonus(Pawn pawn)
        {
            if (pawn == null)
            {
                return 0f;
            }

            return GetEquippedWeaponEnchantValue(pawn, "MeleeDodge")
                   + GetWornApparelEnchantTotal(pawn, "ApparelDodge", 0f);
        }

        public static float GetTotalConsciousnessBonus(Pawn pawn)
        {
            // "Проясняющий" удалён из активной версии мода.
            // Возвращаю 0 даже для старых предметов, чтобы старые сейвы не получали кривой Consciousness эффект.
            return 0f;
        }

        public static float GetPawnDefBaseStatValue(Pawn pawn, string statDefName, float fallback)
        {
            if (pawn == null || pawn.def == null)
            {
                return fallback;
            }

            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statDefName);
            if (statDef == null)
            {
                return fallback;
            }

            float result = statDef.defaultBaseValue;
            if (result <= 0f)
            {
                result = fallback;
            }

            List<StatModifier> statBases = pawn.def.statBases;
            if (statBases != null)
            {
                for (int i = 0; i < statBases.Count; i++)
                {
                    if (statBases[i].stat == statDef)
                    {
                        result = statBases[i].value;
                        break;
                    }
                }
            }

            if (result <= 0f)
            {
                result = fallback;
            }

            return result;
        }

        public static void NotifyResonanceChanged(Thing thing)
        {
            if (thing == null)
            {
                return;
            }

            ClearStatCachesForThing(thing, RelevantThingStatNames);

            Apparel apparel = thing as Apparel;
            if (apparel != null && apparel.Wearer != null)
            {
                NotifyPawnResonanceChanged(apparel.Wearer);
            }

            ThingWithComps thingWithComps = thing as ThingWithComps;
            if (thingWithComps != null)
            {
                CompEquippable equippable = thingWithComps.TryGetComp<CompEquippable>();
                if (equippable != null && equippable.PrimaryVerb != null && equippable.PrimaryVerb.CasterPawn != null)
                {
                    NotifyPawnResonanceChanged(equippable.PrimaryVerb.CasterPawn);
                }
            }
        }

        public static void NotifyPawnResonanceChanged(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            ClearStatCachesForThing(pawn, RelevantPawnStatNames);
            MaintainPawnResonance(pawn);
            TryDirtyPawnCapacities(pawn);
        }

        public static void MaintainPawnResonance(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            ClearLegacyMoodMemory(pawn);
            ResonanceMoodMemoryUtility.Sync(pawn);
            ResonanceHediffUtility.RemoveClarifyingHediff(pawn);
        }

        public static void SyncMoodMemory(Pawn pawn)
        {
            // Оставлено для совместимости со старыми сборками/сохранениями.
            // Актуальный mood бафф работает как обычная Thought_Memory.
            ClearLegacyMoodMemory(pawn);
            ResonanceMoodMemoryUtility.Sync(pawn);
        }

        private static void ClearLegacyMoodMemory(Pawn pawn)
        {
            ResonanceMoodMemoryUtility.ClearLegacyMemory(pawn);
        }

        public static void TryDirtyPawnCapacities(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.capacities == null)
            {
                return;
            }

            try
            {
                object capacities = pawn.health.capacities;
                MethodInfo method = AccessTools.Method(capacities.GetType(), "Notify_CapacityLevelsDirty");
                if (method != null)
                {
                    method.Invoke(capacities, null);
                }
            }
            catch (Exception ex)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] Failed to dirty pawn capacities: " +
                                                     ex.Message);
            }
        }

        private static void ClearStatCachesForThing(Thing thing, string[] statNames)
        {
            if (thing == null || statNames == null)
            {
                return;
            }

            for (int i = 0; i < statNames.Length; i++)
            {
                StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statNames[i]);
                if (statDef == null)
                {
                    continue;
                }

                TryClearStatWorkerCache(statDef, thing);
            }
        }

        private static void TryClearStatWorkerCache(StatDef statDef, Thing thing)
        {
            try
            {
                StatWorker worker = statDef.Worker;
                if (worker == null)
                {
                    return;
                }

                MethodInfo clearForThing =
                    AccessTools.Method(worker.GetType(), "ClearCacheForThing", new Type[] { typeof(Thing) })
                    ?? AccessTools.Method(typeof(StatWorker), "ClearCacheForThing", new Type[] { typeof(Thing) });
                if (clearForThing != null)
                {
                    clearForThing.Invoke(clearForThing.IsStatic ? null : worker, new object[] { thing });
                    return;
                }

                MethodInfo clearFor =
                    AccessTools.Method(worker.GetType(), "ClearCacheFor", new Type[] { typeof(Thing) })
                    ?? AccessTools.Method(typeof(StatWorker), "ClearCacheFor", new Type[] { typeof(Thing) });
                if (clearFor != null)
                {
                    clearFor.Invoke(clearFor.IsStatic ? null : worker, new object[] { thing });
                    return;
                }

                MethodInfo resetCache = AccessTools.Method(worker.GetType(), "ResetCache")
                                        ?? AccessTools.Method(typeof(StatWorker), "ResetCache");
                if (resetCache != null)
                {
                    resetCache.Invoke(resetCache.IsStatic ? null : worker, null);
                }
            }
            catch (Exception ex)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] Failed to clear stat cache for " +
                                                     statDef.defName + ": " + ex.Message);
            }
        }
    }
}
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class ResonanceStatPartInjector
    {
        public static void Inject()
        {
            int added = 0;

            added += AddThingPart("MeleeWeapon_DamageMultiplier", "MeleeDamage", false, "Острый");
            added += AddThingPart("MeleeWeapon_CooldownMultiplier", "MeleeCooldown", true, "Проворный");
            added += AddThingPart("RangedWeapon_DamageMultiplier", "RangedDamage", false, "Точный");
            added += AddThingPart("RangedWeapon_Cooldown", "RangedCooldown", true, "Быстрый");

            // Прикладистый не является обычным StatDef предмета: реальное уменьшение
            // времени изготовки делает Harmony-патч на Stance_Warmup, а в инфокарте
            // предмета добавляется отдельная строка из CompEnchantments.

            added += AddThingPart("ArmorRating_Sharp", "ApparelArmor", false, "Укреплённый");
            added += AddThingPart("ArmorRating_Blunt", "ApparelArmor", false, "Укреплённый");

            // Драчливый и Прицельный работают как gear stat offset, а не как прямое увеличение уровня навыка.
            // Это ближе к ванильным эффектам вроде Драчливости / стрелкового визора.
            added += AddEquippedWeaponOffsetPart("MeleeHitChance", "MeleeSkill", "Драчливый");
            added += AddEquippedWeaponOffsetPart("ShootingAccuracyPawn", "RangedSkill", "Прицельный");

            // Уклонение тоже выводится в инфокарте пешки через MeleeDodgeChance.
            added += AddEquippedWeaponOffsetPart("MeleeDodgeChance", "MeleeDodge", "Дуэлянт");
            added += AddWornApparelOffsetPart("MeleeDodgeChance", "ApparelDodge", 0f, "Гибкий");

            // MaxHitPoints нельзя надёжно менять одним StatPart: Thing.MaxHitPoints в инспекте
            // использует property. Поэтому реальная прочность меняется Harmony-патчем
            // Patch_Thing_MaxHitPoints.

            // Бонус от надетых вещей. Для скорости считается прибавка от базовой скорости пешки.
            added += AddWornApparelPart("MoveSpeed", "ApparelMoveSpeed", false, 0f, "Лёгкий", true, "MoveSpeed", 4.6f);
            added += AddWornApparelPart("IncomingDamageFactor", "ApparelToughness", true, 0f, "Стойкий");

            MasterworkResonanceConfig.LogMessage("[MasterworkResonance] Added " + added + " resonance StatParts.");
        }

        private static int AddThingPart(string statDefName, string enchantmentId, bool isReduction, string label)
        {
            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statDefName);
            if (statDef == null)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] StatDef not found: " + statDefName);
                return 0;
            }

            if (statDef.parts == null)
            {
                statDef.parts = new List<StatPart>();
            }

            for (int i = 0; i < statDef.parts.Count; i++)
            {
                StatPart_ResonanceThing existing = statDef.parts[i] as StatPart_ResonanceThing;
                if (existing != null && existing.EnchantmentId == enchantmentId)
                {
                    return 0;
                }
            }

            statDef.parts.Add(new StatPart_ResonanceThing
            {
                EnchantmentId = enchantmentId,
                IsReduction = isReduction,
                ExplanationLabel = label
            });

            return 1;
        }

        private static int AddEquippedWeaponPart(string statDefName, string enchantmentId, bool isReduction,
            string label)
        {
            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statDefName);
            if (statDef == null)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] StatDef not found: " + statDefName);
                return 0;
            }

            if (statDef.parts == null)
            {
                statDef.parts = new List<StatPart>();
            }

            for (int i = 0; i < statDef.parts.Count; i++)
            {
                StatPart_ResonanceEquippedWeapon existing = statDef.parts[i] as StatPart_ResonanceEquippedWeapon;
                if (existing != null && existing.EnchantmentId == enchantmentId)
                {
                    return 0;
                }
            }

            statDef.parts.Add(new StatPart_ResonanceEquippedWeapon
            {
                EnchantmentId = enchantmentId,
                IsReduction = isReduction,
                ExplanationLabel = label
            });

            return 1;
        }

        private static int AddEquippedWeaponOffsetPart(string statDefName, string enchantmentId, string label)
        {
            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statDefName);
            if (statDef == null)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] StatDef not found: " + statDefName);
                return 0;
            }

            if (statDef.parts == null)
            {
                statDef.parts = new List<StatPart>();
            }

            for (int i = 0; i < statDef.parts.Count; i++)
            {
                StatPart_ResonanceEquippedWeaponOffset existing =
                    statDef.parts[i] as StatPart_ResonanceEquippedWeaponOffset;
                if (existing != null && existing.EnchantmentId == enchantmentId)
                {
                    return 0;
                }
            }

            statDef.parts.Add(new StatPart_ResonanceEquippedWeaponOffset
            {
                EnchantmentId = enchantmentId,
                ExplanationLabel = label
            });

            return 1;
        }

        private static int AddWornApparelPart(
            string statDefName,
            string enchantmentId,
            bool isReduction,
            float cap,
            string label,
            bool addFromPawnBaseStat = false,
            string baseStatDefName = null,
            float fallbackBaseValue = 0f)
        {
            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statDefName);
            if (statDef == null)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] StatDef not found: " + statDefName);
                return 0;
            }

            if (statDef.parts == null)
            {
                statDef.parts = new List<StatPart>();
            }

            for (int i = 0; i < statDef.parts.Count; i++)
            {
                StatPart_ResonanceWornApparel existing = statDef.parts[i] as StatPart_ResonanceWornApparel;
                if (existing != null && existing.EnchantmentId == enchantmentId)
                {
                    return 0;
                }
            }

            statDef.parts.Add(new StatPart_ResonanceWornApparel
            {
                EnchantmentId = enchantmentId,
                IsReduction = isReduction,
                Cap = cap,
                ExplanationLabel = label,
                AddFromPawnBaseStat = addFromPawnBaseStat,
                BaseStatDefName = baseStatDefName,
                FallbackBaseValue = fallbackBaseValue
            });

            return 1;
        }

        private static int AddWornApparelOffsetPart(string statDefName, string enchantmentId, float cap, string label)
        {
            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statDefName);
            if (statDef == null)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] StatDef not found: " + statDefName);
                return 0;
            }

            if (statDef.parts == null)
            {
                statDef.parts = new List<StatPart>();
            }

            for (int i = 0; i < statDef.parts.Count; i++)
            {
                StatPart_ResonanceWornApparelOffset existing = statDef.parts[i] as StatPart_ResonanceWornApparelOffset;
                if (existing != null && existing.EnchantmentId == enchantmentId)
                {
                    return 0;
                }
            }

            statDef.parts.Add(new StatPart_ResonanceWornApparelOffset
            {
                EnchantmentId = enchantmentId,
                Cap = cap,
                ExplanationLabel = label
            });

            return 1;
        }
    }
}
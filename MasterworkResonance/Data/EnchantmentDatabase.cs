using System.Collections.Generic;
using Verse;

namespace MasterworkResonance
{
    public static class EnchantmentDatabase
    {
        private static readonly List<EnchantmentOption> AllOptions = new List<EnchantmentOption>
        {
            // Чанты для предметов с качеством шедевр/легенда
            new EnchantmentOption(
                "MeleeDamage",
                "Острый",
                EnchantTarget.MeleeWeapon,
                0.01f,
                0.07f,
                EnchantValueFormat.PercentBonus,
                "Увеличивает урон ближнего оружия."),

            new EnchantmentOption(
                "MeleeCooldown",
                "Проворный",
                EnchantTarget.MeleeWeapon,
                0.01f,
                0.15f,
                EnchantValueFormat.PercentReduction,
                "Уменьшает время между ударами ближнего оружия."),

            new EnchantmentOption(
                "MeleeMood",
                "Воодушевляющий",
                EnchantTarget.MeleeWeapon,
                1f,
                5f,
                EnchantValueFormat.FlatBonus,
                "Пока оружие экипировано, повышает настроение носителя."),

            new EnchantmentOption(
                "MeleeSkill",
                "Драчливый",
                EnchantTarget.MeleeWeapon,
                1f,
                5f,
                EnchantValueFormat.FlatBonus,
                "Пока оружие экипировано, повышает шанс попасть в ближнем бою."),

            new EnchantmentOption(
                "MeleeDodge",
                "Дуэлянт",
                EnchantTarget.MeleeWeapon,
                1f,
                4f,
                EnchantValueFormat.FlatBonus,
                "Пока оружие экипировано, даёт плоский бонус к уклонению в ближнем бою."),

            new EnchantmentOption(
                "RangedDamage",
                "Точный",
                EnchantTarget.RangedWeapon,
                0.01f,
                0.10f,
                EnchantValueFormat.PercentBonus,
                "Увеличивает урон дальнобойного оружия."),

            new EnchantmentOption(
                "RangedCooldown",
                "Быстрый",
                EnchantTarget.RangedWeapon,
                0.01f,
                0.25f,
                EnchantValueFormat.PercentReduction,
                "Уменьшает время между выстрелами."),

            new EnchantmentOption(
                "RangedWarmup",
                "Прикладистый",
                EnchantTarget.RangedWeapon,
                0.01f,
                0.25f,
                EnchantValueFormat.PercentReduction,
                "Уменьшает время прицеливания оружия."),

            new EnchantmentOption(
                "RangedMood",
                "Собранный",
                EnchantTarget.RangedWeapon,
                1f,
                5f,
                EnchantValueFormat.FlatBonus,
                "Пока оружие экипировано, повышает настроение носителя."),

            new EnchantmentOption(
                "RangedSkill",
                "Прицельный",
                EnchantTarget.RangedWeapon,
                1f,
                5f,
                EnchantValueFormat.FlatBonus,
                "Пока оружие экипировано, повышает точность стрельбы носителя."),

            new EnchantmentOption(
                "ApparelMoveSpeed",
                "Лёгкий",
                EnchantTarget.Apparel,
                0.01f,
                0.07f,
                EnchantValueFormat.PercentBonus,
                "Пока предмет надет, увеличивает скорость передвижения носителя."),

            new EnchantmentOption(
                "ApparelArmor",
                "Укреплённый",
                EnchantTarget.Apparel,
                0.01f,
                0.10f,
                EnchantValueFormat.PercentBonus,
                "Укрепляет защиту предмета от острого и тупого урона."),

            new EnchantmentOption(
                "ApparelDurability",
                "Прочный",
                EnchantTarget.Apparel,
                0.01f,
                1.00f,
                EnchantValueFormat.PercentBonus,
                "Увеличивает максимальную прочность предмета."),

            new EnchantmentOption(
                "ApparelMood",
                "Вдохновляющий",
                EnchantTarget.Apparel,
                1f,
                4f,
                EnchantValueFormat.FlatBonus,
                "Пока предмет надет, повышает настроение носителя."),

            new EnchantmentOption(
                "ApparelToughness",
                "Стойкий",
                EnchantTarget.Apparel,
                0.01f,
                0.07f,
                EnchantValueFormat.PercentBonus,
                "Пока предмет надет, снижает получаемый урон."),

            new EnchantmentOption(
                "ApparelDodge",
                "Гибкий",
                EnchantTarget.Apparel,
                1f,
                7f,
                EnchantValueFormat.FlatBonus,
                "Пока предмет надет, даёт плоский бонус к уклонению в ближнем бою."),

            // Удалено из активного пула
            new EnchantmentOption(
                "ApparelConsciousness",
                "Проясняющий",
                EnchantTarget.Apparel,
                0.01f,
                0.05f,
                EnchantValueFormat.PercentBonus,
                "Удалённый резонанс. Больше не роллится и не даёт эффекта.",
                false),

            new EnchantmentOption(
                "ApparelPainReduction",
                "Обезболивающий",
                EnchantTarget.Apparel,
                0.01f,
                0.20f,
                EnchantValueFormat.PercentReduction,
                "Пока предмет надет, снижает боль носителя. Складывается с другими такими предметами вплоть до 100%."),
        };

        public static List<EnchantmentOption> GetOptionsFor(ThingDef def)
        {
            List<EnchantmentOption> result = new List<EnchantmentOption>();

            for (int i = 0; i < AllOptions.Count; i++)
            {
                EnchantmentOption option = AllOptions[i];
                if (option.CanRoll && option.Matches(def))
                {
                    result.Add(option);
                }
            }

            return result;
        }

        public static EnchantmentOption GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            for (int i = 0; i < AllOptions.Count; i++)
            {
                if (AllOptions[i].Id == id)
                {
                    return AllOptions[i];
                }
            }

            return null;
        }
    }
}
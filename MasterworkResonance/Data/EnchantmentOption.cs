using Verse;

namespace MasterworkResonance
{
    public enum EnchantTarget
    {
        MeleeWeapon,
        RangedWeapon,
        Apparel
    }

    public enum EnchantValueFormat
    {
        PercentBonus,
        PercentReduction,
        FlatBonus
    }

    public sealed class EnchantmentOption
    {
        public readonly string Id;
        public readonly string Label;
        public readonly EnchantTarget Target;
        public readonly float MinValue;
        public readonly float MaxValue;
        public readonly EnchantValueFormat Format;
        public readonly string Description;
        public readonly bool CanRoll;

        public string DisplayLabel
        {
            get { return ResonanceTranslation.EnchantmentLabel(Id, Label); }
        }

        public string DisplayDescription
        {
            get { return ResonanceTranslation.EnchantmentDescription(Id, Description); }
        }

        public EnchantmentOption(
            string id,
            string label,
            EnchantTarget target,
            float maxValue,
            EnchantValueFormat format,
            string description,
            bool canRoll = true)
            : this(id, label, target, GetDefaultMinValue(format, maxValue), maxValue, format, description, canRoll)
        {
        }

        public EnchantmentOption(
            string id,
            string label,
            EnchantTarget target,
            float minValue,
            float maxValue,
            EnchantValueFormat format,
            string description,
            bool canRoll = true)
        {
            Id = id;
            Label = label;
            Target = target;
            MinValue = minValue;
            MaxValue = maxValue;
            Format = format;
            Description = description;
            CanRoll = canRoll;
        }

        private static float GetDefaultMinValue(EnchantValueFormat format, float maxValue)
        {
            if (format == EnchantValueFormat.FlatBonus)
            {
                return 1f;
            }

            // Процентные резонансы роллятся от 1% до своего максимума
            if (maxValue >= 0.01f)
            {
                return 0.01f;
            }

            return maxValue;
        }

        public bool Matches(ThingDef def)
        {
            if (def == null)
            {
                return false;
            }

            if (Target == EnchantTarget.MeleeWeapon)
            {
                return def.IsMeleeWeapon;
            }

            if (Target == EnchantTarget.RangedWeapon)
            {
                return def.IsRangedWeapon;
            }

            return def.IsApparel;
        }

        public float RollValue()
        {
            if (MaxValue <= MinValue)
            {
                return MaxValue;
            }

            // Настроение в игре читается лучше как целое число: +1, +2 ... +5.
            if (Format == EnchantValueFormat.FlatBonus && Id != null && Id.Contains("Mood"))
            {
                return Rand.RangeInclusive((int)MinValue, (int)MaxValue);
            }

            return Rand.Range(MinValue, MaxValue);
        }


        public float RollValueDeterministic(int seed)
        {
            if (MaxValue <= MinValue)
            {
                return MaxValue;
            }

            if (Format == EnchantValueFormat.FlatBonus && Id != null && Id.Contains("Mood"))
            {
                return ResonanceDeterministicRandom.RangeInclusive(seed, (int)MinValue, (int)MaxValue);
            }

            return ResonanceDeterministicRandom.RangeFloat(seed, MinValue, MaxValue);
        }

        public float GetRollPercent(float rolledValue)
        {
            if (MaxValue <= MinValue)
            {
                return 1f;
            }

            float normalized = (rolledValue - MinValue) / (MaxValue - MinValue);
            if (normalized < 0f)
            {
                return 0f;
            }

            if (normalized > 1f)
            {
                return 1f;
            }

            return normalized;
        }

        public string FormatValue(float value)
        {
            if (Format == EnchantValueFormat.FlatBonus)
            {
                return "+" + value.ToString("0.0");
            }

            if (Format == EnchantValueFormat.PercentReduction)
            {
                return "-" + (value * 100f).ToString("0.#") + "%";
            }

            return "+" + (value * 100f).ToString("0.#") + "%";
        }

        public string FormatRange()
        {
            if (MaxValue <= MinValue)
            {
                return FormatValue(MaxValue);
            }

            if (Format == EnchantValueFormat.PercentReduction)
            {
                return "-" + (MinValue * 100f).ToString("0.#") + "% — -" + (MaxValue * 100f).ToString("0.#") + "%";
            }

            if (Format == EnchantValueFormat.PercentBonus)
            {
                return "+" + (MinValue * 100f).ToString("0.#") + "% — +" + (MaxValue * 100f).ToString("0.#") + "%";
            }

            return "+" + MinValue.ToString("0.0") + " — +" + MaxValue.ToString("0.0");
        }

        public string FormatMultiplierExplanation(float value)
        {
            if (Format == EnchantValueFormat.PercentReduction)
            {
                return "x" + (1f - value).ToString("0.###");
            }

            if (Format == EnchantValueFormat.PercentBonus)
            {
                return "x" + (1f + value).ToString("0.###");
            }

            return FormatValue(value);
        }
    }
}
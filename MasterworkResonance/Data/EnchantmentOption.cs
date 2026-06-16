using System;
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

        public bool RollsWholeNumbers
        {
            get
            {
                return Format == EnchantValueFormat.PercentBonus
                       || Format == EnchantValueFormat.PercentReduction
                       || IsWholeNumberRollId(Id);
            }
        }

        public float WholeNumberRollStep
        {
            get
            {
                if (!RollsWholeNumbers)
                {
                    return 0f;
                }

                if (Format == EnchantValueFormat.FlatBonus)
                {
                    return 1f;
                }
                
                return 0.01f;
            }
        }

        public float RoundToAllowedValue(float value)
        {
            if (RollsWholeNumbers)
            {
                return RoundToStep(value, WholeNumberRollStep);
            }

            if (Format == EnchantValueFormat.FlatBonus)
            {
                return RoundToStep(value, 0.1f);
            }
            
            return RoundToStep(value, 0.01f);
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
            
            if (maxValue >= 0.01f)
            {
                return 0.01f;
            }

            return maxValue;
        }

        public float EffectiveMinValue
        {
            get { return MasterworkResonanceMod.Settings.GetMinValue(this); }
        }

        public float EffectiveMaxValue
        {
            get { return MasterworkResonanceMod.Settings.GetMaxValue(this); }
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
            float min = EffectiveMinValue;
            float max = EffectiveMaxValue;
            SanitizeRange(ref min, ref max);

            if (RollsWholeNumbers)
            {
                return RollWholeNumberValue(min, max, false, 0);
            }

            if (max <= min)
            {
                return max;
            }

            return Rand.Range(min, max);
        }

        public float RollValueDeterministic(int seed)
        {
            float min = EffectiveMinValue;
            float max = EffectiveMaxValue;
            SanitizeRange(ref min, ref max);

            if (RollsWholeNumbers)
            {
                return RollWholeNumberValue(min, max, true, seed);
            }

            if (max <= min)
            {
                return max;
            }

            return ResonanceDeterministicRandom.RangeFloat(seed, min, max);
        }

        public float GetRollPercent(float rolledValue)
        {
            float min = EffectiveMinValue;
            float max = EffectiveMaxValue;
            SanitizeRange(ref min, ref max);

            if (max <= min)
            {
                return 1f;
            }

            float normalized = (rolledValue - min) / (max - min);
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
                if (RollsWholeNumbers && IsMoodRollId(Id))
                {
                    return "+" + RoundToInt(value).ToString();
                }
                
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
            float min = EffectiveMinValue;
            float max = EffectiveMaxValue;
            SanitizeRange(ref min, ref max);

            if (max <= min)
            {
                return FormatValue(max);
            }

            if (Format == EnchantValueFormat.PercentReduction)
            {
                return "-" + (min * 100f).ToString("0.#") + "% — -" + (max * 100f).ToString("0.#") + "%";
            }

            if (Format == EnchantValueFormat.PercentBonus)
            {
                return "+" + (min * 100f).ToString("0.#") + "% — +" + (max * 100f).ToString("0.#") + "%";
            }

            return "+" + min.ToString("0.0") + " — +" + max.ToString("0.0");
        }

        private float RollWholeNumberValue(float min, float max, bool deterministic, int seed)
        {
            float step = WholeNumberRollStep;
            if (step <= 0f)
            {
                return max;
            }

            int minTick = RoundToInt(min / step);
            int maxTick = RoundToInt(max / step);
            if (maxTick < minTick)
            {
                maxTick = minTick;
            }

            int rolledTick = deterministic
                ? ResonanceDeterministicRandom.RangeInclusive(seed, minTick, maxTick)
                : Rand.RangeInclusive(minTick, maxTick);

            return rolledTick * step;
        }

        private static bool IsWholeNumberRollId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            return IsMoodRollId(id)
                   || id == "ApparelDodge"
                   || id == "ApparelToughness"
                   || id == "MeleeDodge"
                   || id == "MeleeSkill"
                   || id == "RangedSkill";
        }


        private static bool IsMoodRollId(string id)
        {
            return !string.IsNullOrEmpty(id) && id.Contains("Mood");
        }

        private static float RoundToStep(float value, float step)
        {
            if (step <= 0f)
            {
                return value;
            }

            return (float)(Math.Round(value / step, MidpointRounding.AwayFromZero) * step);
        }

        private static void SanitizeRange(ref float min, ref float max)
        {
            if (min < 0f)
            {
                min = 0f;
            }

            if (max < 0f)
            {
                max = 0f;
            }

            if (max < min)
            {
                max = min;
            }
        }

        private static int RoundToInt(float value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
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
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public sealed class MasterworkResonanceSettings : ModSettings
    {
        public const float DefaultMasterworkAwakeningChance = 0.40f;
        public const float DefaultLegendaryAwakeningChance = 0.80f;
        public const float DefaultOptionWeight = 1f;
        public const float MinOptionWeight = 0.1f;
        public const float MaxOptionWeight = 10f;

        public float masterworkAwakeningChance = DefaultMasterworkAwakeningChance;
        public float legendaryAwakeningChance = DefaultLegendaryAwakeningChance;
        public bool enableDevModeEnchantment = false;

        private Dictionary<string, float> minValues = new Dictionary<string, float>();
        private Dictionary<string, float> maxValues = new Dictionary<string, float>();
        private Dictionary<string, float> rollWeights = new Dictionary<string, float>();
        private Dictionary<string, bool> enabledOverrides = new Dictionary<string, bool>();

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref masterworkAwakeningChance, "masterworkAwakeningChance", DefaultMasterworkAwakeningChance);
            Scribe_Values.Look(ref legendaryAwakeningChance, "legendaryAwakeningChance", DefaultLegendaryAwakeningChance);
            Scribe_Values.Look(ref enableDevModeEnchantment, "enableDevModeEnchantment", false);

            Scribe_Collections.Look(ref minValues, "minValues", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref maxValues, "maxValues", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref rollWeights, "rollWeights", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref enabledOverrides, "enabledOverrides", LookMode.Value, LookMode.Value);

            EnsureDictionaries();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ClampAll();
            }
        }

        public void EnsureDictionaries()
        {
            if (minValues == null)
            {
                minValues = new Dictionary<string, float>();
            }

            if (maxValues == null)
            {
                maxValues = new Dictionary<string, float>();
            }

            if (rollWeights == null)
            {
                rollWeights = new Dictionary<string, float>();
            }

            if (enabledOverrides == null)
            {
                enabledOverrides = new Dictionary<string, bool>();
            }
        }

        public void ClampAll()
        {
            EnsureDictionaries();

            masterworkAwakeningChance = RoundChanceToWholePercent(masterworkAwakeningChance);
            legendaryAwakeningChance = RoundChanceToWholePercent(legendaryAwakeningChance);

            List<EnchantmentOption> options = EnchantmentDatabase.GetAllOptions(true);
            for (int i = 0; i < options.Count; i++)
            {
                EnchantmentOption option = options[i];
                if (option == null)
                {
                    continue;
                }

                float min = option.RoundToAllowedValue(GetMinValue(option));
                float max = option.RoundToAllowedValue(GetMaxValue(option));
                SanitizeRange(option, ref min, ref max);

                SetMinValue(option, min);
                SetMaxValue(option, max);
                SetRollWeight(option, GetRollWeightRaw(option));
            }
        }

        public float GetAwakeningChance(QualityCategory quality)
        {
            if (quality == QualityCategory.Legendary)
            {
                return RoundChanceToWholePercent(legendaryAwakeningChance);
            }

            if (quality == QualityCategory.Masterwork)
            {
                return RoundChanceToWholePercent(masterworkAwakeningChance);
            }

            return 0f;
        }

        public float GetMinValue(EnchantmentOption option)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return 0f;
            }

            float value;
            if (minValues.TryGetValue(option.Id, out value))
            {
                return value;
            }

            return option.MinValue;
        }

        public float GetMaxValue(EnchantmentOption option)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return 0f;
            }

            float value;
            if (maxValues.TryGetValue(option.Id, out value))
            {
                return value;
            }

            return option.MaxValue;
        }

        public void SetMinValue(EnchantmentOption option, float value)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return;
            }

            if (value < 0f)
            {
                value = 0f;
            }

            minValues[option.Id] = value;
        }

        public void SetMaxValue(EnchantmentOption option, float value)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return;
            }

            if (value < 0f)
            {
                value = 0f;
            }

            maxValues[option.Id] = value;
        }

        public bool IsOptionEnabled(EnchantmentOption option)
        {
            if (option == null || !option.CanRoll || string.IsNullOrEmpty(option.Id))
            {
                return false;
            }

            EnsureDictionaries();

            bool enabled;
            if (enabledOverrides.TryGetValue(option.Id, out enabled))
            {
                return enabled;
            }

            return true;
        }

        public void SetOptionEnabled(EnchantmentOption option, bool enabled)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return;
            }

            enabledOverrides[option.Id] = enabled;
        }

        public float GetRollWeight(EnchantmentOption option)
        {
            if (!IsOptionEnabled(option))
            {
                return 0f;
            }

            return GetRollWeightRaw(option);
        }

        public float GetRollWeightRaw(EnchantmentOption option)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return 0f;
            }

            float value;
            if (rollWeights.TryGetValue(option.Id, out value))
            {
                return ClampWeight(value);
            }

            return DefaultOptionWeight;
        }

        public void SetRollWeight(EnchantmentOption option, float value)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return;
            }

            rollWeights[option.Id] = ClampWeight(value);
        }

        public void ResetAwakeningChances()
        {
            masterworkAwakeningChance = DefaultMasterworkAwakeningChance;
            legendaryAwakeningChance = DefaultLegendaryAwakeningChance;
        }

        public void ResetDevModeTools()
        {
            enableDevModeEnchantment = false;
        }

        public void ResetOptionToDefaults(EnchantmentOption option)
        {
            EnsureDictionaries();

            if (option == null || string.IsNullOrEmpty(option.Id))
            {
                return;
            }

            minValues.Remove(option.Id);
            maxValues.Remove(option.Id);
            rollWeights.Remove(option.Id);
            enabledOverrides.Remove(option.Id);
        }

        public void ResetAllToDefaults()
        {
            ResetAwakeningChances();
            ResetDevModeTools();

            EnsureDictionaries();
            minValues.Clear();
            maxValues.Clear();
            rollWeights.Clear();
            enabledOverrides.Clear();
        }

        private static void SanitizeRange(EnchantmentOption option, ref float min, ref float max)
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

        private static float RoundChanceToWholePercent(float value)
        {
            value = Clamp01(value);
            return (float)Math.Round(value * 100f) / 100f;
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

        private static float ClampWeight(float value)
        {
            if (value < MinOptionWeight)
            {
                return MinOptionWeight;
            }

            if (value > MaxOptionWeight)
            {
                return MaxOptionWeight;
            }

            return value;
        }
    }
}

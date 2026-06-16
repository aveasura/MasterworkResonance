using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MasterworkResonance
{
    public sealed class MasterworkResonanceMod : Mod
    {
        public static MasterworkResonanceSettings Settings = new MasterworkResonanceSettings();

        private const float BottomBarHeight = 40f;
        private const float SectionGap = 12f;
        private const float CardGap = 10f;
        private const float CardPadding = 12f;
        private const float ChanceBlockHeight = 222f;
        private const float OptionCardHeight = 312f;
        private const float TargetHeaderHeight = 34f;

        private Vector2 scrollPosition;

        public MasterworkResonanceMod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<MasterworkResonanceSettings>();
            Settings.EnsureDictionaries();
        }

        public override string SettingsCategory()
        {
            return "Masterwork Resonance";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (Settings == null)
            {
                Settings = GetSettings<MasterworkResonanceSettings>();
            }

            Settings.EnsureDictionaries();

            Rect bottomRect = new Rect(inRect.x, inRect.yMax - BottomBarHeight, inRect.width, BottomBarHeight);
            Rect scrollRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - BottomBarHeight - 8f);

            List<EnchantmentOption> options = EnchantmentDatabase.GetAllOptions(false);
            float viewHeight = CalculateViewHeight(options, scrollRect.width - 18f);
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 18f, viewHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);

            float curY = 0f;
            DrawIntro(viewRect, ref curY);
            DrawAwakeningSettings(viewRect, ref curY);
            DrawSectionTitle(viewRect, ref curY, ResonanceTranslation.Translate("SettingsRollRanges", "Resonance roll ranges"));

            EnchantTarget? lastTarget = null;
            for (int i = 0; i < options.Count; i++)
            {
                EnchantmentOption option = options[i];
                if (option == null)
                {
                    continue;
                }

                if (lastTarget == null || lastTarget.Value != option.Target)
                {
                    lastTarget = option.Target;
                    DrawTargetHeader(viewRect, ref curY, GetTargetLabel(option.Target));
                }

                DrawOptionSettingsCard(viewRect, ref curY, option);
            }

            Widgets.EndScrollView();

            float buttonWidth = 340f;
            Rect resetAllRect = new Rect(bottomRect.x, bottomRect.y + 5f, buttonWidth, 30f);
            if (Widgets.ButtonText(resetAllRect, ResonanceTranslation.Translate(
                    "SettingsResetAll",
                    "Restore mod defaults")))
            {
                Settings.ResetAllToDefaults();
            }

            Settings.ClampAll();
        }

        private static float CalculateViewHeight(List<EnchantmentOption> options, float width)
        {
            int targetHeaders = 0;
            EnchantTarget? lastTarget = null;

            for (int i = 0; i < options.Count; i++)
            {
                EnchantmentOption option = options[i];
                if (option == null)
                {
                    continue;
                }

                if (lastTarget == null || lastTarget.Value != option.Target)
                {
                    lastTarget = option.Target;
                    targetHeaders++;
                }
            }
            
            return 84f + ChanceBlockHeight + 44f + targetHeaders * TargetHeaderHeight + options.Count * (OptionCardHeight + CardGap) + 40f;
        }

        private static void DrawIntro(Rect viewRect, ref float curY)
        {
            Text.Font = GameFont.Small;
            string intro = ResonanceTranslation.Translate(
                "SettingsIntro",
                "Configure Masterwork Resonance rolls. Existing resonant items keep their rolled value; these settings affect future rolls.");
            string multiplayerNote = ResonanceTranslation.Translate(
                "SettingsMultiplayerNote",
                "Multiplayer note: every player should use the same settings to keep deterministic crafting rolls identical.");

            float introHeight = Text.CalcHeight(intro, viewRect.width);
            Widgets.Label(new Rect(0f, curY, viewRect.width, introHeight), intro);
            curY += introHeight + 6f;

            float noteHeight = Text.CalcHeight(multiplayerNote, viewRect.width);
            Widgets.Label(new Rect(0f, curY, viewRect.width, noteHeight), multiplayerNote);
            curY += noteHeight + SectionGap;
        }

        private static void DrawAwakeningSettings(Rect viewRect, ref float curY)
        {
            Rect cardRect = new Rect(0f, curY, viewRect.width, ChanceBlockHeight);
            Widgets.DrawMenuSection(cardRect);

            Rect innerRect = Contract(cardRect, CardPadding);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(innerRect);

            DrawAwakeningChance(listing,
                ResonanceTranslation.Translate("SettingsMasterworkChance", "Masterwork awakening chance"),
                ref Settings.masterworkAwakeningChance,
                MasterworkResonanceSettings.DefaultMasterworkAwakeningChance);

            DrawAwakeningChance(listing,
                ResonanceTranslation.Translate("SettingsLegendaryChance", "Legendary awakening chance"),
                ref Settings.legendaryAwakeningChance,
                MasterworkResonanceSettings.DefaultLegendaryAwakeningChance);

            listing.Gap(4f);
            bool enableDevModeEnchantment = Settings.enableDevModeEnchantment;
            listing.CheckboxLabeled(
                ResonanceTranslation.Translate("SettingsEnableDevModeEnchantment", "Enable dev-mode resonance tools"),
                ref enableDevModeEnchantment,
                ResonanceTranslation.Translate(
                    "SettingsEnableDevModeEnchantmentTooltip",
                    "Adds a dev gizmo to generate or reroll resonance on valid items. Works only while RimWorld Dev mode is enabled."));
            Settings.enableDevModeEnchantment = enableDevModeEnchantment;

            if (listing.ButtonText(ResonanceTranslation.Translate(
                    "SettingsResetAwakeningChances",
                    "Restore default chances")))
            {
                Settings.ResetAwakeningChances();
            }

            listing.End();
            curY += ChanceBlockHeight + SectionGap;
        }

        private static void DrawSectionTitle(Rect viewRect, ref float curY, string label)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, curY, viewRect.width, 32f), label);
            Text.Font = GameFont.Small;
            curY += 34f;
        }

        private static void DrawTargetHeader(Rect viewRect, ref float curY, string label)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, curY + 4f, viewRect.width, 24f), label);
            curY += TargetHeaderHeight;
        }

        private static void DrawOptionSettingsCard(Rect viewRect, ref float curY, EnchantmentOption option)
        {
            Rect cardRect = new Rect(0f, curY, viewRect.width, OptionCardHeight);
            Widgets.DrawMenuSection(cardRect);

            Rect innerRect = Contract(cardRect, CardPadding);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(innerRect);

            bool enabled = Settings.IsOptionEnabled(option);
            float min = Settings.GetMinValue(option);
            float max = Settings.GetMaxValue(option);
            float weight = Settings.GetRollWeightRaw(option);
            float sliderMax = GetSliderMax(option);

            Text.Font = GameFont.Small;
            listing.Label(option.DisplayLabel + " — " + option.FormatRange());
            listing.Label(option.DisplayDescription);

            listing.CheckboxLabeled(
                ResonanceTranslation.Translate("SettingsEnabled", "Enabled"),
                ref enabled,
                ResonanceTranslation.Translate("SettingsEnabledTooltip", "If disabled, this resonance will not roll on newly crafted items."));
            Settings.SetOptionEnabled(option, enabled);

            listing.Label(ResonanceTranslation.Translate("SettingsWeight", "Roll weight") + ": " + weight.ToString("0.#") + " (" +
                          ResonanceTranslation.Translate("SettingsWeightHint", "default 1; minimum 0.1") + ")");
            weight = listing.Slider(weight, MasterworkResonanceSettings.MinOptionWeight, MasterworkResonanceSettings.MaxOptionWeight);
            Settings.SetRollWeight(option, Round(weight, 0.1f));

            listing.Gap(4f);
            listing.Label(ResonanceTranslation.Translate("SettingsMin", "Minimum") + ": " + FormatSettingValue(option, min));
            min = listing.Slider(min, 0f, sliderMax);
            min = RoundForOption(option, min);

            listing.Label(ResonanceTranslation.Translate("SettingsMax", "Maximum") + ": " + FormatSettingValue(option, max));
            max = listing.Slider(max, 0f, sliderMax);
            max = RoundForOption(option, max);

            if (max < min)
            {
                max = min;
            }

            Settings.SetMinValue(option, min);
            Settings.SetMaxValue(option, max);

            listing.Gap(6f);
            if (listing.ButtonText(ResonanceTranslation.Translate(
                    "SettingsResetThisRoll",
                    "Restore default resonance values")))
            {
                Settings.ResetOptionToDefaults(option);
            }

            listing.End();
            curY += OptionCardHeight + CardGap;
        }

        private static Rect Contract(Rect rect, float padding)
        {
            return new Rect(rect.x + padding, rect.y + padding, rect.width - padding * 2f, rect.height - padding * 2f);
        }

        private static void DrawAwakeningChance(Listing_Standard listing, string label, ref float chance, float defaultValue)
        {
            chance = Clamp01(chance);
            float percent = chance * 100f;

            percent = Round(percent, 1f);

            listing.Label(label + ": " + percent.ToString("0") + "% " +
                          ResonanceTranslation.Translate("SettingsDefault", "default") + " " +
                          (defaultValue * 100f).ToString("0") + "%");
            percent = listing.Slider(percent, 0f, 100f);
            chance = Clamp01(Round(percent, 1f) / 100f);
        }

        private static string GetTargetLabel(EnchantTarget target)
        {
            if (target == EnchantTarget.MeleeWeapon)
            {
                return ResonanceTranslation.Translate("SettingsMeleeWeapon", "Melee weapons");
            }

            if (target == EnchantTarget.RangedWeapon)
            {
                return ResonanceTranslation.Translate("SettingsRangedWeapon", "Ranged weapons");
            }

            return ResonanceTranslation.Translate("SettingsApparel", "Armor and apparel");
        }

        private static float GetSliderMax(EnchantmentOption option)
        {
            if (option.Format == EnchantValueFormat.FlatBonus)
            {
                return Mathf.Max(20f, option.MaxValue * 4f);
            }

            return Mathf.Max(1f, option.MaxValue * 4f);
        }

        private static float RoundForOption(EnchantmentOption option, float value)
        {
            if (option == null)
            {
                return value;
            }

            return option.RoundToAllowedValue(value);
        }

        private static string FormatSettingValue(EnchantmentOption option, float value)
        {
            return option.FormatValue(value);
        }

        private static float Round(float value, float step)
        {
            if (step <= 0f)
            {
                return value;
            }

            return Mathf.Round(value / step) * step;
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

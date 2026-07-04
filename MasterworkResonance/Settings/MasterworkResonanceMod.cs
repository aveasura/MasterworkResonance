using System;
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
        private const float RaiderGearQualityBlockHeight = 258f;
        private const float RaiderResonanceBlockHeight = 222f;
        private const float OptionCardHeight = 312f;
        private const float TargetHeaderHeight = 34f;
        private const float FeedbackBlockHeight = 148f;
        private const float ResetAllBlockHeight = 58f;
        private const float ResetAllButtonHeight = 30f;
        private const float ResetAllButtonWidth = 340f;
        private const string WorkshopUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=3741570565";

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

            Rect scrollRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - BottomBarHeight - 8f);

            List<EnchantmentOption> options = EnchantmentDatabase.GetAllOptions(false);
            float viewHeight = CalculateViewHeight(options, scrollRect.width - 18f);
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 18f, viewHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);

            float curY = 0f;
            DrawFeedbackBlock(viewRect, ref curY);
            DrawResetAllBlock(viewRect, ref curY);
            DrawIntro(viewRect, ref curY);
            DrawAwakeningSettings(viewRect, ref curY);
            DrawRaiderGearQualitySettings(viewRect, ref curY);
            DrawRaiderResonanceSettings(viewRect, ref curY);
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
            
            return 84f + ChanceBlockHeight + SectionGap + RaiderGearQualityBlockHeight + SectionGap + RaiderResonanceBlockHeight + SectionGap + 44f
                   + targetHeaders * TargetHeaderHeight
                   + options.Count * (OptionCardHeight + CardGap)
                   + FeedbackBlockHeight + SectionGap
                   + ResetAllBlockHeight + SectionGap
                   + 40f;
        }

        private static void DrawIntro(Rect viewRect, ref float curY)
        {
            Text.Font = GameFont.Small;
            string intro = CleanForLabel(ResonanceTranslation.Translate(
                "SettingsIntro",
                "Configure Masterwork Resonance rolls. Existing resonant items keep their rolled value; these settings affect future rolls."));
            string multiplayerNote = CleanForLabel(ResonanceTranslation.Translate(
                "SettingsMultiplayerNote",
                "Multiplayer note: every player should use the same settings to keep deterministic crafting rolls identical."));

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

            DrawAwakeningChance(listing, innerRect.width,
                ResonanceTranslation.Translate("SettingsMasterworkChance", "Masterwork awakening chance"),
                ref Settings.masterworkAwakeningChance,
                MasterworkResonanceSettings.DefaultMasterworkAwakeningChance);

            DrawAwakeningChance(listing, innerRect.width,
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

        private static void DrawRaiderGearQualitySettings(Rect viewRect, ref float curY)
        {
            Rect cardRect = new Rect(0f, curY, viewRect.width, RaiderGearQualityBlockHeight);
            Widgets.DrawMenuSection(cardRect);

            Rect innerRect = Contract(cardRect, CardPadding);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(innerRect);

            Text.Font = GameFont.Medium;
            ListingLabel(listing, ResonanceTranslation.Translate("SettingsRaiderGearQualityTitle", "Raider gear quality"), innerRect.width);
            Text.Font = GameFont.Small;

            ListingLabel(listing, ResonanceTranslation.Translate(
                "SettingsRaiderGearQualityDescription",
                "Optional experimental feature. Only hostile non-player combat pawns can have generated weapon and apparel quality upgraded. Disabled by default."), innerRect.width);

            bool enableRaiderGearQuality = Settings.enableRaiderGearQuality;
            listing.CheckboxLabeled(
                ResonanceTranslation.Translate("SettingsEnableRaiderGearQuality", "Enable raider gear quality upgrades"),
                ref enableRaiderGearQuality,
                ResonanceTranslation.Translate(
                    "SettingsEnableRaiderGearQualityTooltip",
                    "When enabled, hostile raiders can have generated weapons and apparel upgraded to Masterwork or Legendary quality. Traders, guests and allies are ignored."));
            Settings.enableRaiderGearQuality = enableRaiderGearQuality;

            DrawAwakeningChance(listing, innerRect.width,
                ResonanceTranslation.Translate("SettingsRaiderGearQualityUpgradeChance", "Quality upgrade chance"),
                ref Settings.raiderGearQualityUpgradeChance,
                MasterworkResonanceSettings.DefaultRaiderGearQualityUpgradeChance);

            DrawAwakeningChance(listing, innerRect.width,
                ResonanceTranslation.Translate("SettingsRaiderGearLegendaryChance", "Legendary chance after upgrade"),
                ref Settings.raiderGearLegendaryChance,
                MasterworkResonanceSettings.DefaultRaiderGearLegendaryChance);

            if (listing.ButtonText(ResonanceTranslation.Translate(
                    "SettingsResetRaiderGearQuality",
                    "Restore raider gear quality defaults")))
            {
                Settings.ResetRaiderGearQuality();
            }

            listing.End();
            curY += RaiderGearQualityBlockHeight + SectionGap;
        }

        private static void DrawRaiderResonanceSettings(Rect viewRect, ref float curY)
        {
            Rect cardRect = new Rect(0f, curY, viewRect.width, RaiderResonanceBlockHeight);
            Widgets.DrawMenuSection(cardRect);

            Rect innerRect = Contract(cardRect, CardPadding);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(innerRect);

            Text.Font = GameFont.Medium;
            ListingLabel(listing, ResonanceTranslation.Translate("SettingsRaiderResonanceTitle", "Raider resonance"), innerRect.width);
            Text.Font = GameFont.Small;

            ListingLabel(listing, ResonanceTranslation.Translate(
                "SettingsRaiderResonanceDescription",
                "Optional experimental feature. Only hostile non-player combat pawns can awaken resonance on generated Masterwork or Legendary weapons and apparel. Disabled by default."), innerRect.width);

            bool enableRaiderResonance = Settings.enableRaiderResonance;
            listing.CheckboxLabeled(
                ResonanceTranslation.Translate("SettingsEnableRaiderResonance", "Enable raider resonance"),
                ref enableRaiderResonance,
                ResonanceTranslation.Translate(
                    "SettingsEnableRaiderResonanceTooltip",
                    "When enabled, hostile raiders can awaken resonance on generated Masterwork or Legendary gear. Traders, guests and allies are ignored."));
            Settings.enableRaiderResonance = enableRaiderResonance;

            DrawMultiplier(listing, innerRect.width,
                ResonanceTranslation.Translate("SettingsRaiderResonanceChanceMultiplier", "Resonance chance multiplier"),
                ref Settings.raiderResonanceChanceMultiplier,
                MasterworkResonanceSettings.DefaultRaiderResonanceChanceMultiplier,
                MasterworkResonanceSettings.MinRaiderResonanceChanceMultiplier,
                MasterworkResonanceSettings.MaxRaiderResonanceChanceMultiplier);

            if (listing.ButtonText(ResonanceTranslation.Translate(
                    "SettingsResetRaiderResonance",
                    "Restore raider resonance defaults")))
            {
                Settings.ResetRaiderResonance();
            }

            listing.End();
            curY += RaiderResonanceBlockHeight + SectionGap;
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
            ListingLabel(listing, option.DisplayLabel + " — " + option.FormatRange(), innerRect.width);
            ListingLabel(listing, option.DisplayDescription, innerRect.width);

            listing.CheckboxLabeled(
                ResonanceTranslation.Translate("SettingsEnabled", "Enabled"),
                ref enabled,
                ResonanceTranslation.Translate("SettingsEnabledTooltip", "If disabled, this resonance will not roll on newly crafted items."));
            Settings.SetOptionEnabled(option, enabled);

            ListingLabel(listing, ResonanceTranslation.Translate("SettingsWeight", "Roll weight") + ": " + weight.ToString("0.#") + " (" +
                         ResonanceTranslation.Translate("SettingsWeightHint", "default 1; minimum 0.1") + ")", innerRect.width);
            weight = listing.Slider(weight, MasterworkResonanceSettings.MinOptionWeight, MasterworkResonanceSettings.MaxOptionWeight);
            Settings.SetRollWeight(option, Round(weight, 0.1f));

            listing.Gap(4f);
            ListingLabel(listing, ResonanceTranslation.Translate("SettingsMin", "Minimum") + ": " + FormatSettingValue(option, min), innerRect.width);
            min = listing.Slider(min, 0f, sliderMax);
            min = RoundForOption(option, min);

            ListingLabel(listing, ResonanceTranslation.Translate("SettingsMax", "Maximum") + ": " + FormatSettingValue(option, max), innerRect.width);
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

        private static void DrawFeedbackBlock(Rect viewRect, ref float curY)
        {
            Rect cardRect = new Rect(0f, curY, viewRect.width, FeedbackBlockHeight);
            Widgets.DrawMenuSection(cardRect);

            Rect innerRect = Contract(cardRect, CardPadding);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(innerRect);

            Text.Font = GameFont.Medium;
            ListingLabel(listing, ResonanceTranslation.Translate("SettingsFeedbackTitle", "Feedback"), innerRect.width);
            Text.Font = GameFont.Small;

            ListingLabel(listing, ResonanceTranslation.Translate(
                "SettingsFeedbackText",
                "Have an idea, balance suggestion, or bug report? Leave a comment on the Steam Workshop page."), innerRect.width);

            listing.Gap(4f);
            if (listing.ButtonText(ResonanceTranslation.Translate("SettingsOpenWorkshopPage", "Open Workshop page")))
            {
                Application.OpenURL(WorkshopUrl);
            }

            listing.End();
            curY += FeedbackBlockHeight + SectionGap;
        }

        private static void DrawResetAllBlock(Rect viewRect, ref float curY)
        {
            Rect cardRect = new Rect(0f, curY, viewRect.width, ResetAllBlockHeight);
            Widgets.DrawMenuSection(cardRect);

            Rect innerRect = Contract(cardRect, CardPadding);
            Rect buttonRect = new Rect(innerRect.x, innerRect.y + 2f, ResetAllButtonWidth, ResetAllButtonHeight);

            if (Widgets.ButtonText(buttonRect, ResonanceTranslation.Translate(
                    "SettingsResetAll",
                    "Restore default mod settings")))
            {
                Settings.ResetAllToDefaults();
            }

            curY += ResetAllBlockHeight + SectionGap + 4f;
        }

        private static string CleanForLabel(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return string.Join(" ", text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        }

        private static Rect Contract(Rect rect, float padding)
        {
            return new Rect(rect.x + padding, rect.y + padding, rect.width - padding * 2f, rect.height - padding * 2f);
        }

        private static void ListingLabel(Listing_Standard listing, string label, float width)
        {
            label = CleanForLabel(label);
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            float height = Text.CalcHeight(label, width);
            Rect rect = listing.GetRect(height);
            Widgets.Label(rect, label);
        }

        private static void DrawMultiplier(Listing_Standard listing, float width, string label, ref float value, float defaultValue, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            value = Round(value, 0.1f);

            ListingLabel(listing, label + ": x" + value.ToString("0.#") + " " +
                         ResonanceTranslation.Translate("SettingsDefault", "default") + " x" +
                         defaultValue.ToString("0.#"), width);
            value = listing.Slider(value, min, max);
            value = Round(value, 0.1f);
        }

        private static void DrawAwakeningChance(Listing_Standard listing, float width, string label, ref float chance, float defaultValue)
        {
            chance = Clamp01(chance);
            float percent = chance * 100f;

            percent = Round(percent, 1f);

            ListingLabel(listing, label + ": " + percent.ToString("0") + "% " +
                         ResonanceTranslation.Translate("SettingsDefault", "default") + " " +
                         (defaultValue * 100f).ToString("0") + "%", width);
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

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public class CompEnchantments : ThingComp
    {
        private const int ResonanceDisplayPriority = 99999;

        private bool generated;
        private string enchantmentId;
        private float value;
        private float rollPercent;
        private int tier;
        private bool creationMessageShown;

        public string EnchantmentId
        {
            get { return enchantmentId; }
        }

        public float Value
        {
            get { return value; }
        }

        public float RollPercent
        {
            get { return rollPercent; }
        }

        public int Tier
        {
            get { return tier; }
        }

        public bool HasEnchantment
        {
            get { return generated && !string.IsNullOrEmpty(enchantmentId); }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref generated, "generated", false);
            Scribe_Values.Look(ref enchantmentId, "enchantmentId", null);
            Scribe_Values.Look(ref value, "value", 0f);
            Scribe_Values.Look(ref rollPercent, "rollPercent", 0f);
            Scribe_Values.Look(ref tier, "tier", 0);
            Scribe_Values.Look(ref creationMessageShown, "creationMessageShown", false);
        }

        public bool TryGenerate(Pawn worker = null, bool showMessage = false)
        {
            return TryGenerateInternal(worker, showMessage, false, 0, false);
        }

        public bool TryGenerateDeterministic(Pawn worker, bool showMessage, int seed)
        {
            return TryGenerateInternal(worker, showMessage, true, seed, false);
        }

        public bool TryGenerateDev(Pawn worker = null, bool showMessage = false)
        {
            return TryGenerateInternal(worker, showMessage, false, 0, true);
        }

        public bool TryGenerateForcedDeterministic(Pawn worker, bool showMessage, int seed)
        {
            return TryGenerateInternal(worker, showMessage, true, seed, true);
        }

        private bool TryGenerateInternal(Pawn worker, bool showMessage, bool deterministic, int seed, bool forceAwakening)
        {
            if (HasEnchantment)
            {
                return false;
            }

            if (parent == null || parent.def == null)
            {
                return false;
            }

            CompQuality compQuality = parent.TryGetComp<CompQuality>();
            if (compQuality == null)
            {
                return false;
            }

            QualityCategory quality = compQuality.Quality;
            if (quality != QualityCategory.Masterwork && quality != QualityCategory.Legendary)
            {
                return false;
            }

            if (!forceAwakening && !RollAwakeningChance(quality, deterministic, seed))
            {
                return false;
            }

            List<EnchantmentOption> options = EnchantmentDatabase.GetOptionsFor(parent.def);
            if (options.Count == 0)
            {
                return false;
            }

            EnchantmentOption selected = SelectWeightedOption(options, deterministic, ResonanceDeterministicRandom.Combine(seed, 1001));
            if (selected == null)
            {
                return false;
            }

            value = deterministic
                ? selected.RollValueDeterministic(ResonanceDeterministicRandom.Combine(seed, 2001))
                : selected.RollValue();
            rollPercent = selected.GetRollPercent(value);
            tier = CalculateTier(rollPercent);
            enchantmentId = selected.Id;
            generated = true;

            ApplyPostGenerateMaintenance();

            if (showMessage)
            {
                NotifyCreatedIfNeeded(worker);
            }

            return true;
        }

        private static bool RollAwakeningChance(QualityCategory quality, bool deterministic, int seed)
        {
            float chance = GetAwakeningChance(quality);
            if (chance >= 1f)
            {
                return true;
            }

            if (chance <= 0f)
            {
                return false;
            }

            float roll = deterministic
                ? ResonanceDeterministicRandom.StableRandom01(
                    ResonanceDeterministicRandom.Combine(seed, 501))
                : Rand.Value;

            return roll < chance;
        }

        private static float GetAwakeningChance(QualityCategory quality)
        {
            if (quality == QualityCategory.Legendary)
            {
                return MasterworkResonanceMod.Settings.GetAwakeningChance(quality);
            }

            if (quality == QualityCategory.Masterwork)
            {
                return MasterworkResonanceMod.Settings.GetAwakeningChance(quality);
            }

            return 0f;
        }

        private static EnchantmentOption SelectWeightedOption(List<EnchantmentOption> options, bool deterministic, int seed)
        {
            if (options == null || options.Count == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            for (int i = 0; i < options.Count; i++)
            {
                totalWeight += MasterworkResonanceMod.Settings.GetRollWeight(options[i]);
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float roll = deterministic
                ? ResonanceDeterministicRandom.StableRandom01(seed) * totalWeight
                : Rand.Value * totalWeight;

            float cursor = 0f;
            for (int i = 0; i < options.Count; i++)
            {
                EnchantmentOption option = options[i];
                cursor += MasterworkResonanceMod.Settings.GetRollWeight(option);
                if (roll < cursor)
                {
                    return option;
                }
            }

            return options[options.Count - 1];
        }

        public bool TryGenerateAfterCraft(Pawn worker)
        {
            bool generatedNow = TryGenerate(worker, false);
            if (generatedNow || HasEnchantment)
            {
                NotifyCreatedIfNeeded(worker);
            }

            return generatedNow;
        }

        public bool TryGenerateAfterCraftDeterministic(Pawn worker, int seed)
        {
            bool generatedNow = TryGenerateDeterministic(worker, false, seed);
            if (generatedNow || HasEnchantment)
            {
                NotifyCreatedIfNeeded(worker);
            }

            return generatedNow;
        }

        public void NotifyCreatedIfNeeded(Pawn worker)
        {
            if (!HasEnchantment || creationMessageShown)
            {
                return;
            }

            if (worker == null || worker.Faction != Faction.OfPlayer)
            {
                return;
            }

            creationMessageShown = true;

            Messages.Message(
                ResonanceTranslation.Translate("CreatedResonantItem", "Создан резонирующий предмет") + ": " +
                parent.LabelCap + " — " + GetDisplayLine(),
                parent,
                MessageTypeDefOf.PositiveEvent,
                false);
        }

        public override string CompInspectStringExtra()
        {
            if (!HasEnchantment)
            {
                return null;
            }
            
            return ResonanceTranslation.Translate("ResonancePrefix", "Резонанс") + ": " + GetColoredDisplayLine();
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            IEnumerable<StatDrawEntry> baseEntries = base.SpecialDisplayStats();
            if (baseEntries != null)
            {
                foreach (StatDrawEntry entry in baseEntries)
                {
                    yield return entry;
                }
            }

            if (!HasEnchantment)
            {
                yield break;
            }

            yield return new StatDrawEntry(
                StatCategoryDefOf.Basics,
                ResonanceTranslation.Translate("ResonancePrefix", "Резонанс"),
                GetDisplayLine(),
                GetInfoCardReport(),
                ResonanceDisplayPriority);

            if (enchantmentId == "RangedWarmup")
            {
                float baseWarmup;
                if (TryGetBaseRangedWarmup(out baseWarmup))
                {
                    float adjustedWarmup = baseWarmup * (1f - value);
                    if (adjustedWarmup < 0.01f)
                    {
                        adjustedWarmup = 0.01f;
                    }

                    yield return new StatDrawEntry(
                        StatCategoryDefOf.Basics,
                        ResonanceTranslation.Translate("ResonantWarmupStatLabel", "Изготовка с резонансом"),
                        adjustedWarmup.ToString("0.##") + " " +
                        ResonanceTranslation.Translate("SecondsAbbrev", "сек"),
                        ResonanceTranslation.Translate(
                            "ResonantWarmupStatDescription",
                            "Время изготовки этого оружия с учётом резонанса."),
                        ResonanceDisplayPriority - 1);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerable<Gizmo> baseGizmos = base.CompGetGizmosExtra();
            if (baseGizmos != null)
            {
                foreach (Gizmo gizmo in baseGizmos)
                {
                    yield return gizmo;
                }
            }

            if (!MasterworkResonanceConfig.EnableDevModeEnchantment || !Prefs.DevMode)
            {
                yield break;
            }

            Command_Action command = new Command_Action();
            command.defaultLabel = HasEnchantment
                ? ResonanceTranslation.Translate("DevRerollResonance", "Dev: reroll resonance")
                : ResonanceTranslation.Translate("DevGenerateResonance", "Dev: generate resonance");
            command.defaultDesc = ResonanceTranslation.Translate(
                "DevGenerateResonanceDescription",
                "Generate or reroll Masterwork Resonance on this item. Works only for Masterwork/Legendary weapon or apparel.");
            command.action = delegate
            {
                ClearForDevReroll();

                if (TryGenerateDev(null, false))
                {
                    Messages.Message(
                        ResonanceTranslation.Translate("DevResonance", "Dev resonance") + ": " +
                        parent.LabelCap + " — " + GetDisplayLine(),
                        parent,
                        MessageTypeDefOf.PositiveEvent,
                        false);
                }
                else
                {
                    Messages.Message(
                        ResonanceTranslation.Translate(
                            "DevResonanceFailed",
                            "Резонанс не выдан: нужен предмет качества шедевр/легенда и подходящий тип оружия/одежды."),
                        parent,
                        MessageTypeDefOf.RejectInput,
                        false);
                }
            };

            yield return command;
        }

        public bool TryGetValue(string id, out float result)
        {
            result = 0f;

            if (!HasEnchantment || enchantmentId != id)
            {
                return false;
            }

            result = value;
            return true;
        }

        public string GetDisplayLine()
        {
            EnchantmentOption option = EnchantmentDatabase.GetById(enchantmentId);
            if (option == null)
            {
                return enchantmentId + " " + ToRoman(tier) + " (" + value.ToString("0.###") + ")";
            }

            return option.DisplayLabel + " " + ToRoman(tier) + " (" + option.FormatValue(value) + ")";
        }

        public string GetColoredDisplayLine()
        {
            return Colorize(GetDisplayLine(), GetRollColorHex());
        }

        public string GetShortEffectLine()
        {
            EnchantmentOption option = EnchantmentDatabase.GetById(enchantmentId);
            if (option == null)
            {
                return value.ToString("0.###");
            }

            return option.FormatValue(value);
        }

        private string GetRollColorHex()
        {
            EnchantmentOption option = EnchantmentDatabase.GetById(enchantmentId);

            float normalizedRoll = rollPercent;
            if (normalizedRoll <= 0f && option != null)
            {
                normalizedRoll = option.GetRollPercent(value);
            }

            if (normalizedRoll < 0f)
            {
                normalizedRoll = 0f;
            }

            if (normalizedRoll > 1f)
            {
                normalizedRoll = 1f;
            }

            // 0..33% ролл - бронза, 34..66% - золото-оранжевый, 67..100% - зелёный.
            if (normalizedRoll < 0.34f)
            {
                return "#B89A58";
            }

            if (normalizedRoll < 0.67f)
            {
                return "#FF9E2E";
            }

            return "#59FF59";
        }

        private static string Colorize(string text, string colorHex)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(colorHex))
            {
                return text;
            }

            return "<color=" + colorHex + ">" + text + "</color>";
        }

        private bool TryGetBaseRangedWarmup(out float warmup)
        {
            warmup = 0f;

            if (parent == null || parent.def == null || parent.def.Verbs == null)
            {
                return false;
            }

            for (int i = 0; i < parent.def.Verbs.Count; i++)
            {
                VerbProperties verbProperties = parent.def.Verbs[i];
                if (verbProperties == null)
                {
                    continue;
                }

                if (verbProperties.range > 1.42f && verbProperties.warmupTime > 0f)
                {
                    warmup = verbProperties.warmupTime;
                    return true;
                }
            }

            return false;
        }

        private string GetInfoCardReport()
        {
            EnchantmentOption option = EnchantmentDatabase.GetById(enchantmentId);

            string description = option != null
                ? option.DisplayDescription
                : ResonanceTranslation.Translate(
                    "UnknownResonanceDescription",
                    "Неизвестный резонанс. Возможно, этот предмет был создан старой версией мода.");

            string rollLine = ResonanceTranslation.Translate("RollPositionInRange", "Позиция ролла в диапазоне") +
                              ": " + (rollPercent * 100f).ToString("0.#") + "%. " +
                              ResonanceTranslation.Translate("Tier", "Уровень") + ": " + ToRoman(tier) + ".";
            string valueLine = ResonanceTranslation.Translate("CurrentValue", "Текущее значение") +
                               ": " + GetShortEffectLine() + ".";
            string rangeLine = option != null
                ? ResonanceTranslation.Translate("Range", "Диапазон") + ": " + option.FormatRange() + "."
                : string.Empty;

            return description + "\n\n" + valueLine +
                   (string.IsNullOrEmpty(rangeLine) ? string.Empty : "\n" + rangeLine) + "\n" + rollLine;
        }

        private void ApplyPostGenerateMaintenance()
        {
            if (parent == null)
            {
                return;
            }

            ResonanceUtility.NotifyResonanceChanged(parent);

            // Для чанта прочности сразу поднимаем текущую прочность до нового максимума.
            // Иначе MaxHitPoints уже вырос бы, но свежесозданный предмет мог бы остаться с прежним HP.
            if (HasEnchantment && enchantmentId == "ApparelDurability")
            {
                parent.HitPoints = parent.MaxHitPoints;
                return;
            }

            // Если в Dev Mode перероллили предмет с "Прочного" на другой резонанс,
            // старое текущее HP могло оказаться выше нового максимума.
            if (parent.HitPoints > parent.MaxHitPoints)
            {
                parent.HitPoints = parent.MaxHitPoints;
            }
        }

        private void ClearForDevReroll()
        {
            generated = false;
            enchantmentId = null;
            value = 0f;
            rollPercent = 0f;
            tier = 0;
            creationMessageShown = false;

            ApplyPostGenerateMaintenance();
        }

        private static int CalculateTier(float roll)
        {
            if (roll >= 0.95f)
            {
                return 4;
            }

            if (roll >= 0.75f)
            {
                return 3;
            }

            if (roll >= 0.50f)
            {
                return 2;
            }

            return 1;
        }

        private static string ToRoman(int value)
        {
            if (value <= 1)
            {
                return "I";
            }

            if (value == 2)
            {
                return "II";
            }

            if (value == 3)
            {
                return "III";
            }

            return "IV";
        }
    }
}
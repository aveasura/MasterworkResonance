using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace MasterworkResonance
{
    public static class ResonanceTranslation
    {
        private const string Prefix = "MWR_";

        private static readonly Dictionary<string, string> EnglishFallbacks = new Dictionary<string, string>
        {
            { "ResonancePrefix", "Resonance" },
            { "CreatedResonantItem", "Resonant item created" },
            { "ResonantWarmupStatLabel", "Aiming with resonance" },
            { "SecondsAbbrev", "s" },
            { "ResonantWarmupStatDescription", "This weapon's aiming time after resonance is applied." },
            { "DevGenerateResonance", "Dev: generate resonance" },
            { "DevRerollResonance", "Dev: reroll resonance" },
            {
                "DevGenerateResonanceDescription",
                "Generate or reroll Masterwork Resonance on this item. Works only for Masterwork/Legendary weapon or apparel."
            },
            { "DevResonance", "Dev resonance" },
            {
                "DevResonanceFailed",
                "Resonance was not applied: the item must be masterwork/legendary and must be a valid weapon or apparel item."
            },
            {
                "UnknownResonanceDescription",
                "Unknown resonance. This item may have been created by an older version of the mod."
            },
            { "RollPositionInRange", "Roll position in range" },
            { "Tier", "Tier" },
            { "CurrentValue", "Current value" },
            { "Range", "Range" },
            { "CapSuffix", "(cap)" },
            { "FromBaseValueSuffix", "from base value" },
            { "ResonantEquipment", "Resonant equipment" },
            { "ResonantEquipmentTooltip", "Equipped resonant gear improves mood." },

            { "Enchant_MeleeDamage_Label", "Sharp" },
            { "Enchant_MeleeDamage_Description", "Increases melee weapon damage." },
            { "Enchant_MeleeCooldown_Label", "Swift" },
            { "Enchant_MeleeCooldown_Description", "Reduces the time between melee attacks." },
            { "Enchant_MeleeMood_Label", "Encouraging" },
            { "Enchant_MeleeMood_Description", "While equipped, this weapon improves the wielder's mood." },
            { "Enchant_MeleeSkill_Label", "Brawling" },
            { "Enchant_MeleeSkill_Description", "While equipped, this weapon increases melee hit chance." },
            { "Enchant_MeleeDodge_Label", "Duelist" },
            {
                "Enchant_MeleeDodge_Description",
                "While equipped, this weapon grants a flat bonus to melee dodge chance."
            },

            { "Enchant_RangedDamage_Label", "Accurate" },
            { "Enchant_RangedDamage_Description", "Increases ranged weapon damage." },
            { "Enchant_RangedCooldown_Label", "Rapid" },
            { "Enchant_RangedCooldown_Description", "Reduces the time between shots." },
            { "Enchant_RangedWarmup_Label", "Ergonomic" },
            { "Enchant_RangedWarmup_Description", "Reduces weapon aiming time." },
            { "Enchant_RangedMood_Label", "Focused" },
            { "Enchant_RangedMood_Description", "While equipped, this weapon improves the wielder's mood." },
            { "Enchant_RangedSkill_Label", "Targeting" },
            {
                "Enchant_RangedSkill_Description",
                "While equipped, this weapon increases the wielder's shooting accuracy."
            },

            { "Enchant_ApparelMoveSpeed_Label", "Light" },
            { "Enchant_ApparelMoveSpeed_Description", "While worn, this item increases the wearer's movement speed." },
            { "Enchant_ApparelArmor_Label", "Reinforced" },
            { "Enchant_ApparelArmor_Description", "Reinforces this item against sharp and blunt damage." },
            { "Enchant_ApparelDurability_Label", "Durable" },
            { "Enchant_ApparelDurability_Description", "Increases this item's maximum hit points." },
            { "Enchant_ApparelMood_Label", "Inspiring" },
            { "Enchant_ApparelMood_Description", "While worn, this item improves the wearer's mood." },
            { "Enchant_ApparelToughness_Label", "Stalwart" },
            { "Enchant_ApparelToughness_Description", "While worn, this item reduces incoming damage." },
            { "Enchant_ApparelDodge_Label", "Flexible" },
            { "Enchant_ApparelDodge_Description", "While worn, this item grants a flat bonus to melee dodge chance." },

            { "Enchant_ApparelConsciousness_Label", "Clarifying" },
            { "Enchant_ApparelConsciousness_Description", "Removed resonance. It no longer rolls and has no effect." },
            { "Enchant_ApparelPainReduction_Label", "Pain-dulling" },
            {
                "Enchant_ApparelPainReduction_Description",
                "While worn, this item reduces the wearer's pain by 1%–20%. Stacks with other Pain-dulling items up to 100%."
            },
        };

        public static string Translate(string keySuffix, string fallback)
        {
            string key = Prefix + keySuffix;
            string languageId = TryGetActiveLanguageIdentifier();

            if (IsRussian(languageId))
            {
                if (key.CanTranslate())
                {
                    return key.Translate().ToString();
                }

                return fallback;
            }

            string english;
            if (EnglishFallbacks.TryGetValue(keySuffix, out english))
            {
                return english;
            }

            return fallback;
        }

        public static string EnchantmentLabel(string enchantmentId, string fallback)
        {
            return Translate("Enchant_" + enchantmentId + "_Label", fallback);
        }

        public static string EnchantmentDescription(string enchantmentId, string fallback)
        {
            return Translate("Enchant_" + enchantmentId + "_Description", fallback);
        }

        public static string EnchantmentLabelById(string enchantmentId, string fallback)
        {
            EnchantmentOption option = EnchantmentDatabase.GetById(enchantmentId);
            if (option != null)
            {
                return option.DisplayLabel;
            }

            return fallback;
        }

        private static bool IsRussian(string languageId)
        {
            if (string.IsNullOrEmpty(languageId))
            {
                return false;
            }

            return languageId.IndexOf("Russian", StringComparison.OrdinalIgnoreCase) >= 0
                   || languageId.IndexOf("Рус", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string TryGetActiveLanguageIdentifier()
        {
            string direct = ReadStaticStringMember(typeof(LanguageDatabase), "LangFolderName");
            if (!string.IsNullOrEmpty(direct))
            {
                return direct;
            }

            direct = ReadStaticStringMember(typeof(Prefs), "LangFolderName");
            if (!string.IsNullOrEmpty(direct))
            {
                return direct;
            }

            object activeLanguage = ReadStaticObjectMember(typeof(LanguageDatabase), "activeLanguage");
            if (activeLanguage == null)
            {
                activeLanguage = ReadStaticObjectMember(typeof(LanguageDatabase), "ActiveLanguage");
            }

            if (activeLanguage == null)
            {
                return null;
            }

            string fromLanguage = ReadInstanceStringMember(activeLanguage, "folderName");
            if (!string.IsNullOrEmpty(fromLanguage))
            {
                return fromLanguage;
            }

            fromLanguage = ReadInstanceStringMember(activeLanguage, "FolderName");
            if (!string.IsNullOrEmpty(fromLanguage))
            {
                return fromLanguage;
            }

            fromLanguage = ReadInstanceStringMember(activeLanguage, "friendlyNameEnglish");
            if (!string.IsNullOrEmpty(fromLanguage))
            {
                return fromLanguage;
            }

            fromLanguage = ReadInstanceStringMember(activeLanguage, "FriendlyNameEnglish");
            if (!string.IsNullOrEmpty(fromLanguage))
            {
                return fromLanguage;
            }

            object worker = ReadInstanceObjectMember(activeLanguage, "worker");
            if (worker == null)
            {
                worker = ReadInstanceObjectMember(activeLanguage, "Worker");
            }

            if (worker != null)
            {
                return worker.GetType().FullName;
            }

            return activeLanguage.GetType().FullName;
        }

        private static string ReadStaticStringMember(Type type, string name)
        {
            object value = ReadStaticObjectMember(type, name);
            return value as string;
        }

        private static object ReadStaticObjectMember(Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                       BindingFlags.IgnoreCase;

            try
            {
                PropertyInfo property = type.GetProperty(name, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(null, null);
                }
            }
            catch
            {
            }

            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                {
                    return field.GetValue(null);
                }
            }
            catch
            {
            }

            return null;
        }

        private static string ReadInstanceStringMember(object instance, string name)
        {
            object value = ReadInstanceObjectMember(instance, name);
            return value as string;
        }

        private static object ReadInstanceObjectMember(object instance, string name)
        {
            if (instance == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                       BindingFlags.IgnoreCase;
            Type type = instance.GetType();

            try
            {
                PropertyInfo property = type.GetProperty(name, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(instance, null);
                }
            }
            catch
            {
            }

            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                {
                    return field.GetValue(instance);
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
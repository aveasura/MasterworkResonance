using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class ArmorCapUtility
    {
        // В ваниле броневые StatDef ограничены 2.0 = 200%.
        // Для резонанса это мешает: легендарная катафрактарская броня уже упирается в 200%,
        // Поднять потолок боевых armor rating статов.
        private const float RaisedArmorMaxValue = 10f; // 1000%, 

        public static void RaiseArmorCaps()
        {
            int changed = 0;
            changed += TryRaiseStatMax("ArmorRating_Sharp", RaisedArmorMaxValue);
            changed += TryRaiseStatMax("ArmorRating_Blunt", RaisedArmorMaxValue);

            if (changed > 0)
            {
                MasterworkResonanceConfig.LogMessage("[MasterworkResonance] Raised armor StatDef maxValue for " +
                                                     changed + " stats.");
            }
        }

        private static int TryRaiseStatMax(string statDefName, float newMaxValue)
        {
            StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(statDefName);
            if (statDef == null)
            {
                MasterworkResonanceConfig.LogWarning(
                    "[MasterworkResonance] Cannot raise armor cap, StatDef not found: " + statDefName);
                return 0;
            }

            try
            {
                FieldInfo maxValueField = AccessTools.Field(typeof(StatDef), "maxValue");
                if (maxValueField == null)
                {
                    MasterworkResonanceConfig.LogWarning("[MasterworkResonance] Cannot raise armor cap for " +
                                                         statDefName + ": StatDef.maxValue field not found.");
                    return 0;
                }

                object oldObject = maxValueField.GetValue(statDef);
                float oldValue = oldObject is float ? (float)oldObject : 0f;

                if (oldValue >= newMaxValue)
                {
                    return 0;
                }

                maxValueField.SetValue(statDef, newMaxValue);
                return 1;
            }
            catch (Exception ex)
            {
                MasterworkResonanceConfig.LogWarning("[MasterworkResonance] Failed to raise armor cap for " +
                                                     statDefName + ": " + ex.Message);
                return 0;
            }
        }
    }
}
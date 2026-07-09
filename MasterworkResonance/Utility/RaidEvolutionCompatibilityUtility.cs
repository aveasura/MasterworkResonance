using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MasterworkResonance
{
    public static class RaidEvolutionCompatibilityUtility
    {
        private const string RaidEvolutionPackageId = "aveasura.raidevolution";
        private const string RaidEvolutionModTypeName = "RaidEvolution.Settings.RaidEvolutionMod";
        private const string SettingsMemberName = "Settings";
        private const string IntegrationFieldName = "EnableMasterworkResonanceIntegration";

        public static bool IsRaidEvolutionActive
        {
            get { return ModsConfig.IsActive(RaidEvolutionPackageId); }
        }

        public static bool IsRaidEvolutionIntegrationActive
        {
            get
            {
                if (!IsRaidEvolutionActive)
                {
                    return false;
                }

                bool enabled;
                return TryReadRaidEvolutionIntegration(out enabled) && enabled;
            }
        }

        public static bool ShouldSuppressMwRaiderFeatures
        {
            get { return IsRaidEvolutionIntegrationActive; }
        }

        private static bool TryReadRaidEvolutionIntegration(out bool enabled)
        {
            enabled = false;

            try
            {
                Type modType = AccessTools.TypeByName(RaidEvolutionModTypeName);
                if (modType == null)
                {
                    return false;
                }

                object settings = TryGetStaticProperty(modType, SettingsMemberName) ??
                                  TryGetStaticField(modType, SettingsMemberName);
                if (settings == null)
                {
                    return false;
                }

                object value = TryGetInstanceField(settings, IntegrationFieldName) ??
                               TryGetInstanceProperty(settings, IntegrationFieldName);
                if (value is bool)
                {
                    enabled = (bool)value;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MasterworkResonanceConfig.LogWarning(
                    "[MasterworkResonance] Failed to read Raid Evolution compatibility settings: " + ex.Message);
            }

            return false;
        }

        private static object TryGetStaticProperty(Type type, string name)
        {
            PropertyInfo property = AccessTools.Property(type, name);
            return property == null ? null : property.GetValue(null, null);
        }

        private static object TryGetStaticField(Type type, string name)
        {
            FieldInfo field = AccessTools.Field(type, name);
            return field == null ? null : field.GetValue(null);
        }

        private static object TryGetInstanceProperty(object instance, string name)
        {
            if (instance == null)
            {
                return null;
            }

            PropertyInfo property = AccessTools.Property(instance.GetType(), name);
            return property == null ? null : property.GetValue(instance, null);
        }

        private static object TryGetInstanceField(object instance, string name)
        {
            if (instance == null)
            {
                return null;
            }

            FieldInfo field = AccessTools.Field(instance.GetType(), name);
            return field == null ? null : field.GetValue(instance);
        }
    }
}
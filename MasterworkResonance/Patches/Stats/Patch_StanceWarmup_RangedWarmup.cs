using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch]
    public static class Patch_StanceWarmup_RangedWarmup
    {
        private static MethodBase TargetMethod()
        {
            ConstructorInfo[] constructors =
                typeof(Stance_Warmup).GetConstructors(BindingFlags.Instance | BindingFlags.Public |
                                                      BindingFlags.NonPublic);
            for (int i = 0; i < constructors.Length; i++)
            {
                ParameterInfo[] parameters = constructors[i].GetParameters();
                bool hasTicks = false;
                bool hasVerb = false;

                for (int j = 0; j < parameters.Length; j++)
                {
                    if (parameters[j].ParameterType == typeof(int))
                    {
                        hasTicks = true;
                    }
                    else if (parameters[j].ParameterType == typeof(Verb))
                    {
                        hasVerb = true;
                    }
                }

                if (hasTicks && hasVerb)
                {
                    return constructors[i];
                }
            }

            return null;
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        public static void Prefix(ref int ticks, Verb verb)
        {
            if (ticks <= 1 || verb == null || verb.verbProps == null || verb.verbProps.range <= 1.42f)
            {
                return;
            }

            Thing equipment = verb.EquipmentSource;
            CompEnchantments comp = ResonanceUtility.GetCompFromThing(equipment);
            if (comp == null)
            {
                return;
            }

            float value;
            if (!comp.TryGetValue("RangedWarmup", out value) || value <= 0f)
            {
                return;
            }

            int adjustedTicks = (int)Math.Round(ticks * (1f - value), MidpointRounding.AwayFromZero);
            if (adjustedTicks < 1)
            {
                adjustedTicks = 1;
            }

            ticks = adjustedTicks;
        }
    }
}
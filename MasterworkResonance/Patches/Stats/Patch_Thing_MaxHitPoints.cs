using HarmonyLib;
using UnityEngine;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch(typeof(Thing), "get_MaxHitPoints")]
    public static class Patch_Thing_MaxHitPoints
    {
        public static void Postfix(Thing __instance, ref int __result)
        {
            if (__instance == null || __result <= 0)
            {
                return;
            }

            CompEnchantments comp = ResonanceUtility.GetCompFromThing(__instance);
            if (comp == null)
            {
                return;
            }

            float value;
            if (!comp.TryGetValue("ApparelDurability", out value) || value <= 0f)
            {
                return;
            }

            __result = Mathf.RoundToInt(__result * (1f + value));
            if (__result < 1)
            {
                __result = 1;
            }
        }
    }
}
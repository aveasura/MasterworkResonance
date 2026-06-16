using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    // todo
    // Временный тестовый путь для MVP: если качество вещи выставили после создания
    // например через Dev Mode или внутреннюю генерацию RimWorld, попробовать выдать резонанс.
    [HarmonyPatch(typeof(CompQuality), nameof(CompQuality.SetQuality))]
    public static class Patch_CompQuality_SetQuality
    {
        public static void Postfix(CompQuality __instance)
        {
            if (!MasterworkResonanceConfig.EnableDevModeEnchantment || !Prefs.DevMode)
            {
                return;
            }

            if (__instance == null || __instance.parent == null)
            {
                return;
            }

            CompEnchantments comp = __instance.parent.TryGetComp<CompEnchantments>();
            if (comp == null)
            {
                return;
            }

            comp.TryGenerateDev(null, false);
        }
    }
}
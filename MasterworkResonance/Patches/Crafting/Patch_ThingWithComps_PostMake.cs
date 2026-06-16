using HarmonyLib;
using Verse;

namespace MasterworkResonance
{
    // todo
    // Временный тестовый путь для MVP: позволяет зачаровывать вещи которые появляются не через крафт.
    // Полезно для Dev Mode spawn и для быстрой проверки
    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.PostMake))]
    public static class Patch_ThingWithComps_PostMake
    {
        public static void Postfix(ThingWithComps __instance)
        {
            if (!MasterworkResonanceConfig.EnableDevModeEnchantment || !Prefs.DevMode)
            {
                return;
            }

            if (__instance == null)
            {
                return;
            }

            CompEnchantments comp = __instance.TryGetComp<CompEnchantments>();
            if (comp == null)
            {
                return;
            }

            comp.TryGenerateDev(null, false);
        }
    }
}
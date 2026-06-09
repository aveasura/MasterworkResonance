using System.Reflection;
using HarmonyLib;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch]
    public static class Patch_PlayDataLoader_DoPlayLoad_MasterworkResonance
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("Verse.PlayDataLoader:DoPlayLoad");
        }

        private static void Postfix()
        {
            LongEventHandler.ExecuteWhenFinished(EnchantmentsBootstrap.InjectRuntimeDefs);
        }
    }
}
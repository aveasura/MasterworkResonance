using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch]
    public static class Patch_Apparel_NotifyEquipped
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Apparel), "Notify_Equipped", new Type[] { typeof(Pawn) });
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Apparel __instance, Pawn pawn)
        {
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }

    [HarmonyPatch]
    public static class Patch_Apparel_NotifyUnequipped
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Apparel), "Notify_Unequipped", new Type[] { typeof(Pawn) });
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Apparel __instance, Pawn pawn)
        {
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }
}
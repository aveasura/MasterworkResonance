using System.Reflection;
using HarmonyLib;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch]
    public static class Patch_PawnEquipmentTracker_NotifyEquipmentAdded
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_EquipmentTracker), "pawn");

        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(Pawn_EquipmentTracker))
                .FirstOrDefault(method => method.Name == "Notify_EquipmentAdded");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = PawnField != null ? PawnField.GetValue(__instance) as Pawn : null;
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }

    [HarmonyPatch]
    public static class Patch_PawnEquipmentTracker_NotifyEquipmentRemoved
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_EquipmentTracker), "pawn");

        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(Pawn_EquipmentTracker))
                .FirstOrDefault(method => method.Name == "Notify_EquipmentRemoved");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = PawnField != null ? PawnField.GetValue(__instance) as Pawn : null;
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }

    [HarmonyPatch]
    public static class Patch_PawnEquipmentTracker_NotifyEquipmentLost
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_EquipmentTracker), "pawn");

        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(Pawn_EquipmentTracker))
                .FirstOrDefault(method => method.Name == "Notify_EquipmentLost");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = PawnField != null ? PawnField.GetValue(__instance) as Pawn : null;
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }
}
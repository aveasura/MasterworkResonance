using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch]
    public static class Patch_PawnApparelTracker_NotifyApparelAdded
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_ApparelTracker), "pawn");

        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(Pawn_ApparelTracker))
                .FirstOrDefault(method => method.Name == "Notify_ApparelAdded");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Pawn_ApparelTracker __instance)
        {
            Pawn pawn = PawnField != null ? PawnField.GetValue(__instance) as Pawn : null;
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }

    [HarmonyPatch]
    public static class Patch_PawnApparelTracker_NotifyApparelRemoved
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_ApparelTracker), "pawn");

        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(Pawn_ApparelTracker))
                .FirstOrDefault(method => method.Name == "Notify_ApparelRemoved");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Pawn_ApparelTracker __instance)
        {
            Pawn pawn = PawnField != null ? PawnField.GetValue(__instance) as Pawn : null;
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }

    [HarmonyPatch]
    public static class Patch_PawnApparelTracker_NotifyApparelChanged
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_ApparelTracker), "pawn");

        private static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(Pawn_ApparelTracker))
                .FirstOrDefault(method => method.Name == "Notify_ApparelChanged");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Pawn_ApparelTracker __instance)
        {
            Pawn pawn = PawnField != null ? PawnField.GetValue(__instance) as Pawn : null;
            ResonanceUtility.NotifyPawnResonanceChanged(pawn);
        }
    }
}
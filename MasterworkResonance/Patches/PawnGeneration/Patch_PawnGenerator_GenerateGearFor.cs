using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch]
    public static class Patch_PawnGenerator_GenerateGearFor
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(PawnGenerator),
                "GenerateGearFor",
                new Type[]
                {
                    typeof(Pawn),
                    typeof(PawnGenerationRequest)
                });
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            RaiderGearQualityUtility.TryUpgradeGearForPawn(pawn, request);
        }
    }
}

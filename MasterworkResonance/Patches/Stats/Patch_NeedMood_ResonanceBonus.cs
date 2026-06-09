using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    // Прямой mood-бонус без отображения отдельной строки в списке.
    // Патчим только CurInstantLevel — это целевое/актуальное настроение, которое RimWorld
    // показывает маленькой стрелкой под шкалой. Need.CurLevel не трогать, иначе шкала
    // начинает мгновенно догонять/перелетать цель и визуально забивается в 100%.
    [HarmonyPatch]
    public static class Patch_NeedMood_CurInstantLevel_ResonanceBonus
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Need_Mood), "CurInstantLevel");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        private static void Postfix(Need_Mood __instance, ref float __result)
        {
            ResonanceMoodUtility.ApplyMoodBonus(__instance, ref __result);
        }
    }

    public static class ResonanceMoodUtility
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Need), "pawn");

        public static void ApplyMoodBonus(Need_Mood mood, ref float level)
        {
            Pawn pawn = PawnField != null ? PawnField.GetValue(mood) as Pawn : null;
            if (pawn == null || pawn.Dead)
            {
                return;
            }

            float bonusMoodPoints = ResonanceUtility.GetTotalMoodBonus(pawn);
            if (bonusMoodPoints <= 0f)
            {
                return;
            }

            // RimWorld mood need лежит в диапазоне 0..1, а mood-очки - это обычные +1..+5.
            // Поэтому +5 mood = +0.05 к целевому настроению.
            float bonusLevel = bonusMoodPoints / 100f;
            level = Math.Min(1f, level + bonusLevel);
        }
    }
}
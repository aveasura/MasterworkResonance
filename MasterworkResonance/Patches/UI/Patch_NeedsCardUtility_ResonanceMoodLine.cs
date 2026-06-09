using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MasterworkResonance
{
    // UI-only строка в Needs -> Mood thought list.
    // Реальный mood-бонус остаётся в Patch_NeedMood_ResonanceBonus.cs через Need_Mood.CurInstantLevel.
    // Здесь мы ничего не добавляем в ThoughtHandler и не зависим от XML ThoughtDef:
    // просто рисуем строку в той же области где NeedsCardUtility рисует список.
    [HarmonyPatch(typeof(NeedsCardUtility), "DrawThoughtListing")]
    public static class Patch_NeedsCardUtility_DrawThoughtListing_ResonanceMoodLine
    {
        private const float RowHeight = 20f;
        private const float RowStride = 24f;
        private const float ScrollbarWidth = 16f;
        private const float LabelXOffset = 10f;
        private const float LabelWidth = 225f;

        private const float ValueRightOffset = 258f;

        private const float LabelVerticalExpand = 3f;

        private static readonly List<Thought> tmpThoughtGroups = new List<Thought>();

        private static void Postfix(Rect listingRect, Pawn pawn)
        {
            if (pawn == null || pawn.needs == null || pawn.needs.mood == null)
            {
                return;
            }

            float bonus = ResonanceUtility.GetTotalMoodBonus(pawn);
            if (bonus <= 0f)
            {
                return;
            }

            int vanillaRows = CountVisibleVanillaMoodRows(pawn);

            // Ваниль после каждой строки двигает y на 24f.
            float y = listingRect.y + vanillaRows * RowStride;

            if (y + RowHeight > listingRect.yMax)
            {
                return;
            }

            Rect fullRowRect = new Rect(
                listingRect.x,
                y,
                listingRect.width - ScrollbarWidth,
                RowHeight
            );

            Rect labelRect = new Rect(
                listingRect.x + LabelXOffset,
                y,
                LabelWidth,
                RowHeight
            );

            // Ваниль немного расширяет labelRect по Y
            labelRect.yMin -= LabelVerticalExpand;
            labelRect.yMax += LabelVerticalExpand;

            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            Color oldColor = GUI.color;

            try
            {
                Text.Font = GameFont.Small;

                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = Color.white;
                Widgets.Label(
                    labelRect,
                    ResonanceTranslation.Translate("ResonantEquipment", "Резонансное снаряжение"));

                string valueText = FormatMoodBonus(bonus);

                float valueWidth = Mathf.Ceil(Text.CalcSize(valueText).x) + 2f;

                Rect valueRect = new Rect(
                    listingRect.x + ValueRightOffset - valueWidth,
                    y,
                    valueWidth,
                    RowHeight
                );

                Text.Anchor = TextAnchor.MiddleRight;
                GUI.color = new Color(0.35f, 1f, 0.35f);
                Widgets.Label(valueRect, valueText);

                TooltipHandler.TipRegion(
                    fullRowRect,
                    ResonanceTranslation.Translate(
                        "ResonantEquipmentTooltip",
                        "Экипированное резонирующее снаряжение улучшает настроение.")
                );
            }
            finally
            {
                GUI.color = oldColor;
                Text.Anchor = oldAnchor;
                Text.Font = oldFont;
            }
        }

        private static int CountVisibleVanillaMoodRows(Pawn pawn)
        {
            tmpThoughtGroups.Clear();

            try
            {
                PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder(pawn.needs.mood, tmpThoughtGroups);

                int count = 0;
                for (int i = 0; i < tmpThoughtGroups.Count; i++)
                {
                    Thought thought = tmpThoughtGroups[i];
                    if (thought != null && thought.VisibleInNeedsTab)
                    {
                        count++;
                    }
                }

                return count;
            }
            catch
            {
                // Если в другой версии сигнатура/логика UI отличается,
                // лучше не ломать вкладку нужд. В худшем случае строка будет сверху.
                return 0;
            }
            finally
            {
                tmpThoughtGroups.Clear();
            }
        }

        private static string FormatMoodBonus(float bonus)
        {
            if (Math.Abs(bonus - Mathf.Round(bonus)) < 0.001f)
            {
                return Mathf.RoundToInt(bonus).ToString();
            }

            return bonus.ToString("0.#");
        }
    }
}
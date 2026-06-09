using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    // Legacy worker. The old situational thought is kept only for old save compatibility.
    // Actual mood resonance is now stored as a normal Thought_Memory by ResonanceMoodMemoryUtility.
    public class ThoughtWorker_ResonanceMood : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return ThoughtState.Inactive;
        }
    }

    public static class ResonanceMoodMemoryUtility
    {
        public const string LegacyMoodThoughtDefName = "MasterworkResonance_ResonantEquipment";
        public const string MoodMemoryThoughtDefName = "MasterworkResonance_ResonantEquipmentMemory";

        public static void PrepareDefs()
        {
            ApplyLocalizedStages(LegacyMoodThoughtDefName);
            ApplyLocalizedStages(MoodMemoryThoughtDefName);
        }

        public static void Sync(Pawn pawn)
        {
            if (!CanUseMoodMemory(pawn))
            {
                return;
            }

            ThoughtDef memoryDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(MoodMemoryThoughtDefName);
            if (memoryDef == null)
            {
                return;
            }

            float bonus = ResonanceUtility.GetTotalMoodBonus(pawn);
            if (bonus <= 0f)
            {
                RemoveMemoriesOfDef(pawn, memoryDef);
                return;
            }

            int stageIndex = GetStageIndex(memoryDef, bonus);
            if (HasExactlyOneMemoryAtStage(pawn, memoryDef, stageIndex))
            {
                return;
            }

            RemoveMemoriesOfDef(pawn, memoryDef);

            Thought_Memory memory = ThoughtMaker.MakeThought(memoryDef, stageIndex) as Thought_Memory;
            if (memory != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(memory);
            }
        }

        public static void ClearLegacyMemory(Pawn pawn)
        {
            if (!CanUseMoodMemory(pawn))
            {
                return;
            }

            ThoughtDef legacyDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(LegacyMoodThoughtDefName);
            if (legacyDef != null)
            {
                RemoveMemoriesOfDef(pawn, legacyDef);
            }
        }

        private static bool CanUseMoodMemory(Pawn pawn)
        {
            return pawn != null && !pawn.Dead && pawn.needs != null && pawn.needs.mood != null &&
                   pawn.needs.mood.thoughts != null && pawn.needs.mood.thoughts.memories != null;
        }

        private static int GetStageIndex(ThoughtDef memoryDef, float bonus)
        {
            int moodPoints = (int)Math.Round(bonus, MidpointRounding.AwayFromZero);
            if (moodPoints < 1)
            {
                moodPoints = 1;
            }

            int maxStageIndex = 0;
            if (memoryDef.stages != null && memoryDef.stages.Count > 0)
            {
                maxStageIndex = memoryDef.stages.Count - 1;
            }

            int stageIndex = moodPoints - 1;
            if (stageIndex > maxStageIndex)
            {
                stageIndex = maxStageIndex;
            }

            return stageIndex;
        }

        private static bool HasExactlyOneMemoryAtStage(Pawn pawn, ThoughtDef memoryDef, int stageIndex)
        {
            List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
            if (memories == null)
            {
                return false;
            }

            int count = 0;
            for (int i = 0; i < memories.Count; i++)
            {
                Thought_Memory memory = memories[i];
                if (memory == null || memory.def != memoryDef)
                {
                    continue;
                }

                count++;
                if (memory.CurStageIndex != stageIndex)
                {
                    return false;
                }
            }

            return count == 1;
        }

        private static void RemoveMemoriesOfDef(Pawn pawn, ThoughtDef thoughtDef)
        {
            if (pawn == null || thoughtDef == null || pawn.needs == null || pawn.needs.mood == null ||
                pawn.needs.mood.thoughts == null || pawn.needs.mood.thoughts.memories == null)
            {
                return;
            }

            List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
            if (memories == null)
            {
                return;
            }

            for (int i = memories.Count - 1; i >= 0; i--)
            {
                Thought_Memory memory = memories[i];
                if (memory != null && memory.def == thoughtDef)
                {
                    memories.RemoveAt(i);
                }
            }
        }

        private static void ApplyLocalizedStages(string defName)
        {
            ThoughtDef thoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(defName);
            if (thoughtDef == null || thoughtDef.stages == null)
            {
                return;
            }

            string label = ResonanceTranslation.Translate("ResonantEquipment", "Резонансное снаряжение");
            string description = ResonanceTranslation.Translate(
                "ResonantEquipmentTooltip",
                "Экипированное резонирующее снаряжение улучшает настроение.");

            for (int i = 0; i < thoughtDef.stages.Count; i++)
            {
                ThoughtStage stage = thoughtDef.stages[i];
                if (stage == null)
                {
                    continue;
                }

                stage.label = label;
                stage.description = description;
            }
        }
    }
}
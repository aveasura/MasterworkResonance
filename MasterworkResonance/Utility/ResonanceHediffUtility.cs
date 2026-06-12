using System.Collections.Generic;
using Verse;

namespace MasterworkResonance
{
    public static class ResonanceHediffUtility
    {
        private const string ClarifyingHediffDefName = "MasterworkResonance_ClarifyingConsciousness";
        private const string PainReductionHediffDefName = "MasterworkResonance_PainReduction";
        private const string PainReductionEnchantmentId = "ApparelPainReduction";
        private const float PainReductionCap = 1.00f;

        public static void PrepareDefs()
        {
            // "Проясняющий" удалён. XML HediffDef оставлен только ради совместимости со старыми сохранениями,
            // где hediff мог уже попасть на пешку. Новые эффекты Consciousness больше не чантятся.
            // "Обезболивающий" работает через скрытый/служебный hediff с динамическим PainFactor.
            // Стакается от экипировки до 100%, поэтому полный сет может дать PainFactor x0.00.
        }

        public static float GetPainReductionTotal(Pawn pawn)
        {
            return ResonanceUtility.GetWornApparelEnchantTotal(pawn, PainReductionEnchantmentId, PainReductionCap);
        }

        public static void MaintainPainReductionHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
            {
                return;
            }

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(PainReductionHediffDefName);
            if (hediffDef == null)
            {
                return;
            }

            float total = GetPainReductionTotal(pawn);
            Hediff existing = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];
                if (hediff == null || hediff.def != hediffDef)
                {
                    continue;
                }

                if (existing == null)
                {
                    existing = hediff;
                }
                else
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }

            if (total <= 0f)
            {
                if (existing != null)
                {
                    pawn.health.RemoveHediff(existing);
                }

                return;
            }

            if (existing == null)
            {
                existing = HediffMaker.MakeHediff(hediffDef, pawn);
                pawn.health.AddHediff(existing);
            }

            existing.Severity = total;
        }

        public static void RemoveClarifyingHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
            {
                return;
            }

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(ClarifyingHediffDefName);
            if (hediffDef == null)
            {
                return;
            }

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];
                if (hediff != null && hediff.def == hediffDef)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}
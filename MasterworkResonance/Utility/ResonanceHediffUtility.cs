using System.Collections.Generic;
using Verse;

namespace MasterworkResonance
{
    public static class ResonanceHediffUtility
    {
        private const string ClarifyingHediffDefName = "MasterworkResonance_ClarifyingConsciousness";

        public static void PrepareDefs()
        {
            // "Проясняющий" удалён. XML HediffDef оставлен только ради совместимости со старыми сохранениями,
            // где hediff мог уже попасть на пешку. Новые эффекты Consciousness больше не чантятся.
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
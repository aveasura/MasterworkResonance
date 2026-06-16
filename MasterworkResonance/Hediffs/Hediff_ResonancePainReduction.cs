using Verse;

namespace MasterworkResonance
{
    public class Hediff_ResonancePainReduction : Hediff
    {
        public override float PainFactor
        {
            get
            {
                if (pawn == null)
                {
                    return 1f;
                }

                float total = ResonanceHediffUtility.GetPainReductionTotal(pawn);
                if (total <= 0f)
                {
                    return 1f;
                }

                float factor = 1f - total;
                if (factor < 0f)
                {
                    factor = 0f;
                }

                return factor;
            }
        }
    }
}

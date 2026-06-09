using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public class StatPart_ResonanceWornApparel : StatPart
    {
        public string EnchantmentId;
        public bool IsReduction;
        public float Cap;
        public string ExplanationLabel;

        public bool AddFromPawnBaseStat;
        public string BaseStatDefName;
        public float FallbackBaseValue;

        public override void TransformValue(StatRequest req, ref float val)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
            {
                return;
            }

            float total = ResonanceUtility.GetWornApparelEnchantTotal(pawn, EnchantmentId, Cap);
            if (total <= 0f)
            {
                return;
            }

            if (AddFromPawnBaseStat)
            {
                float baseValue = ResonanceUtility.GetPawnDefBaseStatValue(pawn, BaseStatDefName, FallbackBaseValue);
                float offset = baseValue * total;
                val += IsReduction ? -offset : offset;
                if (val < 0f)
                {
                    val = 0f;
                }

                return;
            }

            val *= IsReduction ? 1f - total : 1f + total;
        }

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
            {
                return null;
            }

            float total = ResonanceUtility.GetWornApparelEnchantTotal(pawn, EnchantmentId, Cap);
            if (total <= 0f)
            {
                return null;
            }

            string sign = IsReduction ? "-" : "+";
            string capText = Cap > 0f && total >= Cap
                ? " " + ResonanceTranslation.Translate("CapSuffix", "(лимит)")
                : string.Empty;
            string baseText = AddFromPawnBaseStat
                ? " " + ResonanceTranslation.Translate("FromBaseValueSuffix", "от базового значения")
                : string.Empty;
            string label = ResonanceTranslation.EnchantmentLabelById(EnchantmentId, ExplanationLabel);
            return ResonanceTranslation.Translate("ResonancePrefix", "Резонанс") + " — " + label +
                   ": " + sign + (total * 100f).ToString("0.#") + "%" + baseText + capText;
        }
    }
}
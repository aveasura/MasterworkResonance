using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public class StatPart_ResonanceWornApparelOffset : StatPart
    {
        public string EnchantmentId;
        public float Cap;
        public string ExplanationLabel;

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

            // Именно плоский stat offset: как у trait/gear offset для MeleeDodgeChance.
            val += total;
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

            string capText = Cap > 0f && total >= Cap
                ? " " + ResonanceTranslation.Translate("CapSuffix", "(лимит)")
                : string.Empty;
            string label = ResonanceTranslation.EnchantmentLabelById(EnchantmentId, ExplanationLabel);
            return ResonanceTranslation.Translate("ResonancePrefix", "Резонанс") + " — " + label +
                   ": +" + total.ToString("0.0") + capText;
        }
    }
}
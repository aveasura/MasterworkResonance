using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public class StatPart_ResonanceEquippedWeapon : StatPart
    {
        public string EnchantmentId;
        public bool IsReduction;
        public string ExplanationLabel;

        public override void TransformValue(StatRequest req, ref float val)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
            {
                return;
            }

            float enchantValue = ResonanceUtility.GetEquippedWeaponEnchantValue(pawn, EnchantmentId);
            if (enchantValue <= 0f)
            {
                return;
            }

            val *= IsReduction ? 1f - enchantValue : 1f + enchantValue;
        }

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
            {
                return null;
            }

            float enchantValue = ResonanceUtility.GetEquippedWeaponEnchantValue(pawn, EnchantmentId);
            if (enchantValue <= 0f)
            {
                return null;
            }

            string sign = IsReduction ? "-" : "+";
            string label = ResonanceTranslation.EnchantmentLabelById(EnchantmentId, ExplanationLabel);
            return ResonanceTranslation.Translate("ResonancePrefix", "Резонанс") + " — " + label +
                   ": " + sign + (enchantValue * 100f).ToString("0.#") + "%";
        }
    }
}
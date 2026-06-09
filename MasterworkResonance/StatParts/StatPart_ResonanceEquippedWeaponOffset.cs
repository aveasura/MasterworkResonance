using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public class StatPart_ResonanceEquippedWeaponOffset : StatPart
    {
        public string EnchantmentId;
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

            // Именно плоский stat offset: как у trait/gear offset для MeleeDodgeChance.
            val += enchantValue;
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

            string label = ResonanceTranslation.EnchantmentLabelById(EnchantmentId, ExplanationLabel);
            return ResonanceTranslation.Translate("ResonancePrefix", "Резонанс") + " — " + label +
                   ": +" + enchantValue.ToString("0.0");
        }
    }
}
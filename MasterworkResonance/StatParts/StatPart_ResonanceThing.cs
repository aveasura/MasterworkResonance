using RimWorld;

namespace MasterworkResonance
{
    public class StatPart_ResonanceThing : StatPart
    {
        public string EnchantmentId;
        public bool IsReduction;
        public string ExplanationLabel;

        public override void TransformValue(StatRequest req, ref float val)
        {
            float enchantValue;
            if (!ResonanceUtility.TryGetThingEnchantValue(req, EnchantmentId, out enchantValue))
            {
                return;
            }

            val *= IsReduction ? 1f - enchantValue : 1f + enchantValue;
        }

        public override string ExplanationPart(StatRequest req)
        {
            float enchantValue;
            if (!ResonanceUtility.TryGetThingEnchantValue(req, EnchantmentId, out enchantValue))
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
using Verse;

namespace MasterworkResonance
{
    public static class MasterworkResonanceConfig
    {
        // Общий переключатель диагностических логов мода.
        public const bool EnableDebugLogging = false;

        // Включает тестовые dev-инструменты резонанса:
        // - gizmo "Dev: generate/reroll resonance" на предмете;
        // - автоматическую выдачу резонанса предметам, созданным/изменённым через Dev Mode spawn/set quality.
        public const bool EnableDevModeEnchantment = false;

        // MP-safe режим генерации резонанса после крафта.
        // Не использует Verse.Rand напрямую и не сдвигает общий RNG RimWorld.
        // Результат строится из стабильного seed предмета/рецепта/пешки.
        public const bool EnableDeterministicCraftRolls = true;

        // Шанс пробуждения резонанса при крафте.
        public const float MasterworkAwakeningChance = 0.40f;
        public const float LegendaryAwakeningChance = 0.80f;

        public static void LogMessage(string message)
        {
            if (EnableDebugLogging)
            {
                Log.Message(message);
            }
        }

        public static void LogWarning(string message)
        {
            if (EnableDebugLogging)
            {
                Log.Warning(message);
            }
        }
    }
}
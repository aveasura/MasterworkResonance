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
        // Сам флаг настраивается через ModSettings и дополнительно требует включённый RimWorld Dev Mode.
        public static bool EnableDevModeEnchantment
        {
            get
            {
                MasterworkResonanceSettings settings = MasterworkResonanceMod.Settings;
                return settings != null && settings.enableDevModeEnchantment;
            }
        }

        // MP-safe режим генерации резонанса после крафта.
        // Не использует Verse.Rand напрямую и не сдвигает общий RNG RimWorld.
        // Результат строится из стабильного seed предмета/рецепта/пешки.
        public const bool EnableDeterministicCraftRolls = true;

        // Шансы пробуждения настраиваются через ModSettings.
        // Дефолты находятся в MasterworkResonanceSettings.
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
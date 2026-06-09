using HarmonyLib;
using Verse;

namespace MasterworkResonance
{
    [StaticConstructorOnStartup]
    public static class EnchantmentsBootstrap
    {
        static EnchantmentsBootstrap()
        {
            Harmony harmony = new Harmony("aveasura.masterworkresonance");
            harmony.PatchAll();

            LongEventHandler.ExecuteWhenFinished(InjectRuntimeDefs);
        }

        public static void InjectRuntimeDefs()
        {
            EnchantableDefInjector.Inject();
            ArmorCapUtility.RaiseArmorCaps();
            ResonanceHediffUtility.PrepareDefs();
            ResonanceMoodMemoryUtility.PrepareDefs();
            ResonanceStatPartInjector.Inject();
        }
    }
}
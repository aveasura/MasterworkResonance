using System;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    [HarmonyPatch]
    public static class Patch_GenRecipe_PostProcessProduct
    {
        private static MethodBase TargetMethod()
        {
            // 1.5 signature. Keep the exact lookup first.
            MethodInfo exactMethod = AccessTools.Method(
                typeof(GenRecipe),
                "PostProcessProduct",
                new Type[]
                {
                    typeof(Thing),
                    typeof(RecipeDef),
                    typeof(Pawn),
                    typeof(Precept_ThingStyle),
                    typeof(ThingStyleDef),
                    typeof(int?)
                }
            );

            if (exactMethod != null)
            {
                return exactMethod;
            }

            // Version-tolerant fallback for RimWorld 1.6+ if Ludeon adds/removes optional parameters.
            // The postfix only needs __result, recipeDef and worker, so Harmony can bind a subset.
            return AccessTools.GetDeclaredMethods(typeof(GenRecipe))
                .FirstOrDefault(method =>
                {
                    if (method == null || method.Name != "PostProcessProduct" || method.ReturnType != typeof(Thing))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    bool hasProduct = false;
                    bool hasRecipe = false;
                    bool hasWorker = false;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType == typeof(Thing))
                        {
                            hasProduct = true;
                        }
                        else if (parameters[i].ParameterType == typeof(RecipeDef))
                        {
                            hasRecipe = true;
                        }
                        else if (parameters[i].ParameterType == typeof(Pawn))
                        {
                            hasWorker = true;
                        }
                    }

                    return hasProduct && hasRecipe && hasWorker;
                });
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        public static void Postfix(Thing __result, RecipeDef recipeDef, Pawn worker)
        {
            Thing thing = UnwrapMinifiedThing(__result);
            if (thing == null)
            {
                return;
            }

            CompEnchantments comp = thing.TryGetComp<CompEnchantments>();
            if (comp == null)
            {
                return;
            }

            if (MasterworkResonanceConfig.EnableDeterministicCraftRolls)
            {
                int seed = ResonanceDeterministicRandom.BuildCraftSeed(thing, recipeDef, worker);
                comp.TryGenerateAfterCraftDeterministic(worker, seed);
                return;
            }

            comp.TryGenerateAfterCraft(worker);
        }

        private static Thing UnwrapMinifiedThing(Thing thing)
        {
            MinifiedThing minifiedThing = thing as MinifiedThing;
            if (minifiedThing != null)
            {
                return minifiedThing.InnerThing;
            }

            return thing;
        }
    }
}
using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public static class ResonanceDeterministicRandom
    {
        public static int BuildCraftSeed(Thing product, RecipeDef recipeDef, Pawn worker)
        {
            unchecked
            {
                int seed = 17;

                seed = Combine(seed, StableStringHash("MasterworkResonance.CraftRoll"));

                if (product != null)
                {
                    seed = Combine(seed, product.thingIDNumber);

                    if (product.def != null)
                    {
                        seed = Combine(seed, StableStringHash(product.def.defName));
                        seed = Combine(seed, product.def.shortHash);
                    }

                    if (product.Map != null)
                    {
                        seed = Combine(seed, product.Map.uniqueID);
                    }

                    seed = Combine(seed, product.Position.x);
                    seed = Combine(seed, product.Position.z);

                    CompQuality compQuality = product.TryGetComp<CompQuality>();
                    if (compQuality != null)
                    {
                        seed = Combine(seed, (int)compQuality.Quality);
                    }
                }

                if (recipeDef != null)
                {
                    seed = Combine(seed, StableStringHash(recipeDef.defName));
                    seed = Combine(seed, recipeDef.shortHash);
                }

                if (worker != null)
                {
                    seed = Combine(seed, worker.thingIDNumber);

                    if (worker.Map != null)
                    {
                        seed = Combine(seed, worker.Map.uniqueID);
                    }

                    seed = Combine(seed, worker.Position.x);
                    seed = Combine(seed, worker.Position.z);
                }

                return seed;
            }
        }

        public static int Combine(int seed, int value)
        {
            unchecked
            {
                return (seed * 397) ^ value;
            }
        }

        public static int StableStringHash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            unchecked
            {
                // FNV-1a 32-bit: стабильный между клиентами/запусками, в отличие от string.GetHashCode().
                uint hash = 2166136261u;
                for (int i = 0; i < text.Length; i++)
                {
                    hash ^= text[i];
                    hash *= 16777619u;
                }

                return (int)hash;
            }
        }

        public static float StableRandom01(int seed)
        {
            unchecked
            {
                uint x = (uint)seed;

                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;

                return (x & 0x00FFFFFF) / 16777216f;
            }
        }

        public static int Range(int seed, int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            int span = maxExclusive - minInclusive;
            int rolled = (int)(StableRandom01(seed) * span);
            if (rolled >= span)
            {
                rolled = span - 1;
            }

            return minInclusive + rolled;
        }

        public static int RangeInclusive(int seed, int minInclusive, int maxInclusive)
        {
            if (maxInclusive <= minInclusive)
            {
                return minInclusive;
            }

            return Range(seed, minInclusive, maxInclusive + 1);
        }

        public static float RangeFloat(int seed, float minInclusive, float maxInclusive)
        {
            if (maxInclusive <= minInclusive)
            {
                return minInclusive;
            }

            return minInclusive + StableRandom01(seed) * (maxInclusive - minInclusive);
        }
    }
}
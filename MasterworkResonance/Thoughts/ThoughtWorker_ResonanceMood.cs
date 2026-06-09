using RimWorld;
using Verse;

namespace MasterworkResonance
{
    public class ThoughtWorker_ResonanceMood : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return ThoughtState.Inactive;
        }
    }
}
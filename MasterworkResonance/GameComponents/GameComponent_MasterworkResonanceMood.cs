using Verse;

namespace MasterworkResonance
{
    public class GameComponent_MasterworkResonanceMood : GameComponent
    {
        private const int SyncIntervalTicks = 250;

        public GameComponent_MasterworkResonanceMood(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (Find.TickManager == null || Find.TickManager.TicksGame % SyncIntervalTicks != 0)
            {
                return;
            }

            if (Find.Maps == null)
            {
                return;
            }

            for (int i = 0; i < Find.Maps.Count; i++)
            {
                Map map = Find.Maps[i];
                if (map == null || map.mapPawns == null || map.mapPawns.AllPawnsSpawned == null)
                {
                    continue;
                }

                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    ResonanceUtility.MaintainPawnResonance(pawn);
                }
            }
        }
    }
}
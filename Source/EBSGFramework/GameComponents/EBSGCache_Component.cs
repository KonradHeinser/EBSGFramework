using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGCache_Component : GameComponent
    {
        public bool loaded;
        public Dictionary<Pawn, Hediff> pawnTerrainComps;

        public void RegisterPawn(Pawn pawn, Hediff hediff)
        {
            pawnTerrainComps.Add(pawn, hediff);
        }

        public EBSGCache_Component(Game game)
        {
            loaded = false;
            pawnTerrainComps = new Dictionary<Pawn, Hediff>();
        }

        public bool RetrievePawnTerrainComp(Pawn pawn, out HediffCompProperties_TerrainCostOverride comp)
        {
            comp = null;
            return false;
        }

        public override void StartedNewGame()
        {
            Initialize();
        }

        public override void LoadedGame()
        {
            Initialize();
        }

        public void Initialize()
        {
            loaded = true;

            if (pawnTerrainComps == null) pawnTerrainComps = new Dictionary<Pawn, Hediff>();
            else pawnTerrainComps.Clear();
        }
    }
}

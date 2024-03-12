using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGCache_Component : GameComponent
    {
        public bool loaded;
        public Dictionary<Pawn, Hediff> pawnTerrainComps;

        public void RegisterTerrainPawn(Pawn pawn, Hediff hediff)
        {
            if (!pawnTerrainComps.ContainsKey(pawn))
                pawnTerrainComps.Add(pawn, hediff);
            else
                pawnTerrainComps[pawn] = hediff;
        }

        public void DeRegisterTerrainPawn(Pawn pawn, Hediff hediff)
        {
            pawnTerrainComps.Remove(pawn);
        }

        public EBSGCache_Component(Game game)
        {
            loaded = false;
            pawnTerrainComps = new Dictionary<Pawn, Hediff>();
        }

        public bool RetrievePawnTerrainComp(Pawn pawn, out HediffCompProperties_TerrainCostOverride comp)
        {
            // Only the first one applies to the pawn

            if (pawnTerrainComps.NullOrEmpty() || !pawnTerrainComps.ContainsKey(pawn) || !EBSGUtilities.HasHediff(pawn, pawnTerrainComps[pawn].def))
            {

            }


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
            // Clears all caches every time
            loaded = true;

            pawnTerrainComps = new Dictionary<Pawn, Hediff>();
        }
    }
}

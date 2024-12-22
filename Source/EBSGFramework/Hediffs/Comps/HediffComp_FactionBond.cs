using System.Collections.Generic;
using Verse;
using RimWorld.Planet;

namespace EBSGFramework
{
    public class HediffComp_FactionBond : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.IsHashIntervalTick(60))
            {
                return;
            }
            int bondedAllies = 0; // 1 means this pawn is the only one with the hediff
            if (parent.pawn.Map != null)
            {
                List<Pawn> allies = parent.pawn.Map.mapPawns.SpawnedPawnsInFaction(parent.pawn.Faction);
                foreach (Pawn ally in allies)
                {
                    if (!ally.Dead && ally.HasHediff(parent.def)) bondedAllies++;
                }
            }
            else if (parent.pawn.GetCaravan() != null)
            {
                Caravan caravan = parent.pawn.GetCaravan();
                foreach (Pawn pawn in caravan.pawns)
                {
                    if (!pawn.Dead && pawn.Faction != null && pawn.Faction == parent.pawn.Faction && pawn.HasHediff(parent.def)) bondedAllies++;
                }
            }
            else bondedAllies = 1;
            parent.Severity = bondedAllies;
        }
    }
}

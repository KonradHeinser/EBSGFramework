using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_FactionBond : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!base.Pawn.IsHashIntervalTick(60))
            {
                return;
            }
            int bondedAllies = 0; // 1 means this pawn is the only one with the hediff
            List<Pawn> allies = parent.pawn.Map.mapPawns.SpawnedPawnsInFaction(parent.pawn.Faction);
            foreach (Pawn ally in allies)
            {
                if (!ally.Dead && ally.health.hediffSet.HasHediff(parent.def)) bondedAllies++;
            }
            parent.Severity = bondedAllies;
        }
    }
}

using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_SeverityByNearbyPawns : HediffComp
    {
        public HediffCompProperties_SeverityByNearbyPawns Props => (HediffCompProperties_SeverityByNearbyPawns)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Props.onlyNonPlayer && Props.onlyPlayer)
            {
                Log.Error(parent.def + ": has both onlyPlayer and onlyNonPlayer, which makes no sense for obvious reasons");
                Pawn.health.RemoveHediff(parent);
                return;
            }

            List<Pawn> list = parent.pawn.Map.mapPawns.AllPawnsSpawned;
            List<Pawn> allies = parent.pawn.Map.mapPawns.SpawnedPawnsInFaction(parent.pawn.Faction);
            int severity = 0;

            if (Props.onlyPlayer)
            {
                foreach (Pawn pawn in allies)
                {
                    if (!pawn.Dead && (pawn == parent.pawn && !Props.includeSelf) || (!pawn.RaceProps.Humanlike && Props.onlyHumanlikes))
                    {
                        continue;
                    }
                    if (pawn.Position.DistanceTo(parent.pawn.Position) <= Props.range)
                    {
                        severity++;
                    }
                }
            }
            else if (Props.onlyNonPlayer)
            {
                foreach (Pawn pawn in list)
                {

                    if (pawn.Dead || (!pawn.RaceProps.Humanlike && Props.onlyHumanlikes) || allies.Contains(pawn))
                    {
                        continue;
                    }
                    if (pawn.Position.DistanceTo(parent.pawn.Position) <= Props.range)
                    {
                        severity++;
                    }
                }
                if (Props.includeSelf) severity++; // Apparently the basic list doesn't include the pawn themselves
            }
            else
            {
                foreach (Pawn pawn in list)
                {
                    if (!pawn.Dead && (pawn == parent.pawn && !Props.includeSelf) || (!pawn.RaceProps.Humanlike && Props.onlyHumanlikes))
                    {
                        continue;
                    }
                    if (pawn.Position.DistanceTo(parent.pawn.Position) <= Props.range)
                    {
                        severity++;
                    }
                }
                if (Props.includeSelf) severity++; // Apparently the basic list doesn't include the pawn themselves
            }


            parent.Severity = severity;
        }
    }
}

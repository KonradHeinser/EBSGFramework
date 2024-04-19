using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_SeverityByNearbyPawns : HediffComp
    {
        public HediffCompProperties_SeverityByNearbyPawns Props => (HediffCompProperties_SeverityByNearbyPawns)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Props.onlyDifferentFaction && Props.onlySameFaction)
            {
                Log.Error(parent.def + ": has both onlySameFaction and onlyDifferentFaction, which makes no sense for obvious reasons");
                Pawn.health.RemoveHediff(parent);
                return;
            }
            float range = Props.range;
            if (Props.rangeStat != null && parent.pawn.GetStatValue(Props.rangeStat) > 0) range = parent.pawn.GetStatValue(Props.rangeStat);

            List<Pawn> list = parent.pawn.Map.mapPawns.AllPawns.Where((Pawn p) => CheckPawn(p, range)).ToList();
            parent.Severity = list.Count;
        }

        public bool CheckPawn(Pawn p, float range)
        {
            if (p.Dead) return false;
            if (!p.RaceProps.Humanlike && Props.onlyHumanlikes) return false;
            if (p == parent.pawn)
            {
                if (!Props.includeSelf) return false;
            }
            else
            {
                if (Props.onlySameFaction && p.Faction != parent.pawn.Faction) return false;
                if (Props.onlyDifferentFaction && (p.Faction == null || p.Faction == parent.pawn.Faction)) return false;
                if (Props.onlyEnemies && !p.HostileTo(parent.pawn)) return false;
            }
            if (p.Position.DistanceTo(parent.pawn.Position) > range) return false;
            return true;
        }
    }
}

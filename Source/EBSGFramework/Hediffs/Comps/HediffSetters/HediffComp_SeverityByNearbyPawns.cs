using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByNearbyPawns : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByNearbyPawns Props => (HediffCompProperties_SeverityByNearbyPawns)props;

        protected override bool MustBeSpawned => true;

        protected override void SetSeverity()
        {
            base.SetSeverity();

            float range = Props.range;
            if (Props.rangeStat != null && Pawn.StatOrOne(Props.rangeStat) > 0)
                range = Pawn.StatOrOne(Props.rangeStat);

            var pawns = Pawn.Map.mapPawns.AllPawns.Where(p => CheckPawn(p, range));
            parent.Severity = pawns.Count();

            ticksToNextCheck = 60;
        }

        public bool CheckPawn(Pawn p, float range)
        {
            if (p.Dead)
                return false;

            if (!p.RaceProps.Humanlike && Props.onlyHumanlikes)
                return false;

            if (p == Pawn)
            {
                if (!Props.includeSelf)
                    return false;
            }
            else
            {
                if (Props.onlySameFaction && p.Faction != Pawn.Faction) 
                        return false;
                if (Props.onlyDifferentFaction && (p.Faction == null || p.Faction == Pawn.Faction)) 
                        return false;
                if (Props.onlyEnemies && !p.HostileTo(Pawn)) 
                        return false;
            }

            return p.Position.DistanceTo(Pawn.Position) < range;
        }
    }
}

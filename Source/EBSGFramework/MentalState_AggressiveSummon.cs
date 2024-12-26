using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class MentalState_AggressiveSummon : MentalState_Manhunter
    {
        public override bool ForceHostileTo(Faction f)
        {
            return pawn.Faction == null || pawn.Faction.HostileTo(f);
        }

        public override bool ForceHostileTo(Thing t)
        {
            if (t is Pawn p && p.RaceProps.Roamer)
                return false;

            return pawn.Faction == null || t.HostileTo(pawn.Faction);
        }
    }
}

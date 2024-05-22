using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalAnyActiveEnemyInMap : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.Spawned) return false;
            Map map = pawn.Map;
            return GenHostility.AnyHostileActiveThreatTo(map, pawn.Faction, false, false);
        }
    }
}

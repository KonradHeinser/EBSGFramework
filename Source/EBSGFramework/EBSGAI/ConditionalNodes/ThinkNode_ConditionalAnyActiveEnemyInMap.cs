using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalAnyActiveEnemyInMap : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            // Faction check is needed due to how any active hostile threat is coded
            if (!pawn.Spawned || pawn.Faction == null) return false;
            Map map = pawn.Map;
            return GenHostility.AnyHostileActiveThreatTo(map, pawn.Faction, false, false);
        }
    }
}

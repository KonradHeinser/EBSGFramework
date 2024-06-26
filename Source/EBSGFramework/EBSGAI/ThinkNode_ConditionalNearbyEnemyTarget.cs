using Verse.AI;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalNearbyEnemyTarget : ThinkNode_Conditional
    {
        private float searchRadius = 40f;

        protected override bool Satisfied(Pawn pawn)
        {
            Thing target = EBSGUtilities.GetCurrentTarget(pawn, autoSearch: true, searchRadius: searchRadius);
            return target != null && target.HostileTo(pawn);
        }
    }
}

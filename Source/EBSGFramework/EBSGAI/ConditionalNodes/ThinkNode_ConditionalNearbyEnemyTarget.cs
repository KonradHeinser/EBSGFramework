using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalNearbyEnemyTarget : ThinkNode_Conditional
    {
        private float searchRadius = 40f;

        protected override bool Satisfied(Pawn pawn)
        {
            Thing target = pawn.GetCurrentTarget(autoSearch: true, searchRadius: searchRadius);
            return target != null;
        }
    }
}

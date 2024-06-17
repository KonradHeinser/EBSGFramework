using Verse.AI;
using Verse;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalNearbyEnemyTarget : ThinkNode_Conditional
    {
        private float searchRadius = 40f;

        protected override bool Satisfied(Pawn pawn)
        {
            return EBSGUtilities.GetCurrentTarget(pawn, autoSearch: true, searchRadius: searchRadius) != null;
        }
    }
}

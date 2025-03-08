using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalNeedTending : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.health.HasHediffsNeedingTend();
        }
    }
}

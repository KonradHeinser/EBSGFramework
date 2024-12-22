using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalTargetIsMechanoid : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            Thing enemy = pawn.GetCurrentTarget(autoSearch: true);
            if (enemy == null) return false;
            if (enemy is Pawn target)
                return target.RaceProps.IsMechanoid;

            return false;
        }
    }
}

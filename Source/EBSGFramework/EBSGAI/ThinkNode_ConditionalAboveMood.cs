using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalAboveMood : ThinkNode_Conditional
    {
        private float minMood = 0.5f;
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.needs?.mood?.CurLevel >= minMood;
        }
    }
}

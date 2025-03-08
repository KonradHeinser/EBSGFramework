using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalNearbyCover : ThinkNode_Conditional
    {
        private float cover = 4;

        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.Spawned) return false;
            return CoverUtility.TotalSurroundingCoverScore(pawn.Position, pawn.Map) >= cover;
        }
    }
}

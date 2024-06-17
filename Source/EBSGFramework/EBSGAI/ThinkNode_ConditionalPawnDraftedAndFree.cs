using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalPawnDraftedAndFree : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.Faction == Faction.OfPlayer)
            {
                return pawn.Drafted && !PawnUtility.PlayerForcedJobNowOrSoon(pawn);
            }
            return false;
        }
    }
}

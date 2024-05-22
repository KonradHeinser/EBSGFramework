using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalHasAnyOfHediff : ThinkNode_Conditional
    {
        private List<HediffDef> hediffs = null;
        protected override bool Satisfied(Pawn pawn)
        {
            foreach (HediffDef hediff in hediffs)
            {
                if (EBSGUtilities.HasHediff(pawn, hediff)) return true;
            }
            return false;
        }
    }
}

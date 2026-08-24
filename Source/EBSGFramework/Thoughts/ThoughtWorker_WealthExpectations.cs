using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_WealthExpectations : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.IsSlave)
                return ThoughtState.Inactive;
            
            ExpectationDef expectationDef = p.MapHeld != null ? ExpectationsUtility.CurrentExpectationFor(p.MapHeld) : ExpectationDefOf.VeryLow;
            if (expectationDef == null)
                return ThoughtState.Inactive;
            
            return ThoughtState.ActiveAtStage(expectationDef.thoughtStage);
        }
    }
}
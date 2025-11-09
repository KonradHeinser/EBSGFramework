using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_HediffsOnRemoval : HediffComp
    {
        public HediffCompProperties_HediffsOnRemoval Props => (HediffCompProperties_HediffsOnRemoval)props;

        public override void CompPostPostRemoved()
        {
            Log.Message("Ding");
            base.CompPostPostRemoved();
            if (!EBSGUtilities.WithinSeverityRanges(parent.Severity, Props.validSeverity))
                return;
            Props.hediffsToGive?.GiveHediffs(null, Pawn);
        }
    }
}

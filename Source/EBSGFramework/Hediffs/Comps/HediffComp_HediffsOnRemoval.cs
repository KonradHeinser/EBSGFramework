using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_HediffsOnRemoval : HediffComp
    {
        public HediffCompProperties_HediffsOnRemoval Props => (HediffCompProperties_HediffsOnRemoval)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (!EBSGUtilities.WithinSeverityRanges(parent.Severity, Props.validSeverity))
                return;
            Props.hediffsToGive?.GiveHediffs(Pawn);
        }
    }
}

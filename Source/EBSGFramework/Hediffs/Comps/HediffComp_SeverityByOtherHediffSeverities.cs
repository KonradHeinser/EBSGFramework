using System;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByOtherHediffSeverities : HediffComp
    {
        public HediffCompProperties_SeverityByOtherHediffSeverities Props => (HediffCompProperties_SeverityByOtherHediffSeverities)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(200)) return;

            float newSeverity = Props.baseSeverity;

            if (!Props.hediffSets.NullOrEmpty())
            {
                foreach (HediffSeverityFactor hediffSet in Props.hediffSets)
                {
                    Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(hediffSet.hediff);
                    if (hediff != null)
                    {
                        float add = hediff.Severity * hediffSet.factor;
                        if (hediffSet.factor < 0) newSeverity += Math.Min(add, hediffSet.minResult);
                        else newSeverity += Math.Max(add, hediffSet.minResult);
                    }
                    else newSeverity += hediffSet.minResult;
                }
            }

            parent.Severity = newSeverity;
        }
    }
}

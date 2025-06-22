using System;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByOtherHediffSeverities : HediffComp
    {
        public HediffCompProperties_SeverityByOtherHediffSeverities Props => (HediffCompProperties_SeverityByOtherHediffSeverities)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(120)) return;

            float newSeverity = Props.baseSeverity;

            foreach (HediffSeverityFactor hediffSet in Props.hediffSets)
            {
                Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(hediffSet.hediff);
                if (hediff != null)
                {
                    float add = hediff.Severity * hediffSet.factor;
                    if (hediffSet.factor < 0) 
                        newSeverity += Math.Max(Math.Min(add, hediffSet.minResult), hediffSet.maxResult);
                    else 
                        newSeverity += Math.Min(Math.Max(add, hediffSet.minResult), hediffSet.maxResult);
                }
                else newSeverity += hediffSet.missingHediffResult;
            }

            parent.Severity = newSeverity;
        }
    }
}

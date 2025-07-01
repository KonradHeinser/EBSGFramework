using System;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByOtherHediffSeverities : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByOtherHediffSeverities Props => (HediffCompProperties_SeverityByOtherHediffSeverities)props;

        protected override void SetSeverity()
        {
            base.SetSeverity();

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
            ticksToNextCheck = 120;
        }
    }
}

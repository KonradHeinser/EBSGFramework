using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompUseEffect_AlterXenotypeByHediffSeverity : CompUseEffect
    {
        public CompProperties_UseEffectAlterXenotypeByHediffSeverity Props => (CompProperties_UseEffectAlterXenotypeByHediffSeverity)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            if (!usedBy.HasHediff(Props.hediff, out Hediff result))
            {
                float initialSeverity = Props.severityChange;
                if (!Props.initialSeverity.NullOrEmpty())
                    foreach (var xeno in Props.initialSeverity)
                        if (usedBy.genes.Xenotype == xeno.xenotype)
                        {
                            initialSeverity = xeno.range.RandomInRange;
                            break;
                        }
                
                if (Props.severityChange == Math.Floor(Props.severityChange))
                    initialSeverity = (int)Math.Floor(initialSeverity);

                usedBy.AddOrAppendHediffs(initialSeverity, Props.severityChange, Props.hediff);
            }
            else
                result.Severity += Props.severityChange;

            Hediff h = usedBy.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
            foreach (var xeno in Props.xenotypes)
                if (xeno.severity.Includes(h.Severity))
                {
                    if (xeno.xenotype != null)
                    {
                        ThingDef filth = xeno.filth ?? Props.filth;
                        IntRange count = xeno.filth != null ? xeno.filthCount : Props.filthCount;
                        usedBy.AlterXenotype(xeno.xenotype, filth, count, xeno.setXenotype, Props.sendMessage);
                    }
                    usedBy.RemoveGenesFromPawn(xeno.removeGenes);
                    usedBy.AddGenesToPawn(xeno.xenotype != null ? !xeno.xenotype.inheritable : !usedBy.genes.Xenotype.inheritable, xeno.addGenes);
                    return;
                }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Props.prerequisites?.ValidPawn(p) == false)
            {
                string failMessage = Props.prerequisites.notMetString?.TranslateOrFormat();
                if (failMessage != null)
                    return failMessage;
                return AcceptanceReport.WasRejected;
            }
            return base.CanBeUsedBy(p);
        }
    }
}

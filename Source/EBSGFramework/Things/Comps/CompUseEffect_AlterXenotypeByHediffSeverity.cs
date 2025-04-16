using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            usedBy.AddOrAppendHediffs(Props.severityChange, Props.severityChange, Props.hediff);
            Hediff h = usedBy.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
            foreach (var xeno in Props.xenotypes)
                if (xeno.severity.Includes(h.Severity))
                {
                    ThingDef filth = xeno.filth ?? Props.filth;
                    IntRange count = xeno.filth != null ? xeno.filthCount : Props.filthCount;
                    usedBy.AlterXenotype(xeno.xenotype, filth, count, xeno.setXenotype, Props.sendMessage);
                    usedBy.RemoveGenesFromPawn(xeno.removeGenes);
                    usedBy.AddGenesToPawn(!xeno.xenotype.inheritable, xeno.addGenes);
                    return;
                }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Props.prerequisites?.ValidPawn(p) == false)
            {
                string failMessage = Props.prerequisites.notMetString?.TranslateOrLiteral();
                if (failMessage != null)
                    return failMessage;
                return AcceptanceReport.WasRejected;
            }
            return base.CanBeUsedBy(p);
        }
    }
}

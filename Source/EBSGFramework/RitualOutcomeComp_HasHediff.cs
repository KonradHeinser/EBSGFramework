using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class RitualOutcomeComp_HasHediff : RitualOutcomeComp_QualitySingleOffset
    {
        [NoTranslate]
        public string roleId;

        public List<List<HediffWithRange>> hediffs = new List<List<HediffWithRange>>();
        
        public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            Pawn pawn = ritual.PawnWithRole(roleId);

            if (pawn != null) 
                return hediffs.Sum(hediff => pawn.PawnHediffRangeNum(hediff));

            return 0;
        }

        public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            Pawn pawn = ritual.PawnWithRole(roleId);

            return pawn != null ? hediffs.Count(h => pawn.PawnHasAnyOfHediffs(h.Select(range => range.hediff).ToList())) : 0;
        }
        
        public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
        {
            if (ritual == null)
                return labelAbstract;
            
            Pawn pawn = ritual?.PawnWithRole(roleId);
            if (pawn == null)
                return null;

            float num = hediffs.Sum(hediff => pawn.PawnHediffRangeNum(hediff));
            string text = ((num < 0f) ? "" : "+");
            return LabelForDesc.Formatted(pawn.Named("PAWN")) + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + num.ToStringPercent()) + ".";
        }

        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
        {
            Pawn pawn = assignments.FirstAssignedPawn(roleId);
            if (pawn == null)
                return null;

            var count = hediffs.Count(h => pawn.PawnHasAnyOfHediffs(h.Select(range => range.hediff).ToList())).ToString();
            if (count == "0")
                return null;
            
            float num = hediffs.Sum(hediff => pawn.PawnHediffRangeNum(hediff));
            return new QualityFactor
            {
                label = label.Formatted(pawn.Named("PAWN")),
                count = count,
                qualityChange = ExpectedOffsetDesc(num > 0f, num),
                positive = (num > 0f),
                quality = num,
                priority = 0f
            };
        }
    }
}

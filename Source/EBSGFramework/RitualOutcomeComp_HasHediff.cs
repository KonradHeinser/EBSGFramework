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
    }
}
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompUseEffect_OffsetComaCapacity : CompUseEffect
    {
        private CompProperties_UseEffectOffsetComaCapacity Props => (CompProperties_UseEffectOffsetComaCapacity)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.genes?.GetGene(Props.gene) is Gene_Coma comaGene)
                comaGene.OffsetCapacity(Props.offset);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!EBSGUtilities.HasRelatedGene(p, Props.gene))
                return "EBSG_Missing".Translate(Props.gene.label);
            return base.CanBeUsedBy(p);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, "EBSG_Capacity".Translate(Props.gene.label).CapitalizeFirst(), Props.offset.ToStringWithSign(), "EBSG_ComaRestCapacityDesc".Translate(), 1010);
        }
    }
}

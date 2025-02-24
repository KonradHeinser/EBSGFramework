using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompUseEffect_AlterGenes : CompUseEffect
    {
        public CompProperties_UseEffectAlterGenes Props => (CompProperties_UseEffectAlterGenes)props;

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

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            usedBy.GainRandomGeneSet(Props.inheritable, Props.removeGenesFromOtherLists, 
                Props.geneSets, Props.alwaysAddedGenes, Props.alwaysRemovedGenes, Props.showMessage);
        }
    }
}

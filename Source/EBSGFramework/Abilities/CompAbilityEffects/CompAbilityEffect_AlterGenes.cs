using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_AlterGenes : CompAbilityEffect
    {
        public new CompProperties_AbilityAlterGenes Props => (CompProperties_AbilityAlterGenes)props;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!(target.Thing is Pawn pawn) || pawn.genes == null)
            {
                if (throwMessages)
                    Messages.Message("EBSG_RequireGenes".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            return base.Valid(target, throwMessages);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = target.Pawn;
            EBSGUtilities.GainRandomGeneSet(pawn, Props.inheritable, Props.removeGenesFromOtherLists, Props.geneSets, Props.alwaysAddedGenes, Props.alwaysRemovedGenes, Props.showMessage);
        }
    }
}

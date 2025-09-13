using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_GiveResource : CompAbilityEffect
    {
        public new CompProperties_AbilityGiveResource Props => (CompProperties_AbilityGiveResource)props;

        protected float GetAmount(Pawn target)
        {
            return Props.amount * (Props.statEffects?.FinalFactor(parent.pawn, target) ?? 1);
        }

        protected bool HasEnoughResource(Pawn target, float? a = null)
        {
            if (!Props.checkLimits)
                return target.genes.GetGene(Props.mainResourceGene) is ResourceGene;

            if (target.genes.GetGene(Props.mainResourceGene) is ResourceGene gene_Resource)
            {
                float amount = a == null ? GetAmount(target) : a.Value;
                if (gene_Resource.Value + amount < 0)
                    return false;
                
                if (gene_Resource.Value + amount > gene_Resource.Max)
                    return false;
                
                return true;
            }
            return false;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Pawn?.genes?.GetGene(Props.mainResourceGene) is ResourceGene resource)
                ResourceGene.OffsetResource(target.Pawn, GetAmount(target.Pawn), resource, null, Props.applyTargetGainStat);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (target.Pawn == null) 
                return false;
            return HasEnoughResource(target.Pawn);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) && Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Pawn?.genes == null) // If it isn't a pawn with genes, it's an obvious no
                return false;

            var amount = GetAmount(target.Pawn);
            if (amount < 0) // If it's a drain, account for abilities reserving resources
                amount -= CompAbilityEffect_ResourceCost.TotalResourceCostOfQueuedAbilities(target.Pawn, Props.mainResourceGene);

            if (!HasEnoughResource(target.Pawn, amount))
            {
                if (!target.Pawn.HasRelatedGene(Props.mainResourceGene))
                {
                    if (throwMessages)
                        Messages.Message("AbilityDisabledNoResourceGene".Translate(target.Pawn, Props.mainResourceGene.LabelCap), target.Pawn, MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                if (amount < 0)
                {
                    if (throwMessages)
                        Messages.Message("AbilityDisabledNoResource".Translate(target.Pawn, Props.mainResourceGene.resourceLabel), target.Pawn, MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                if (throwMessages)
                    Messages.Message("AbilityDisabledTooMuchResource".Translate(target.Pawn, Props.mainResourceGene.resourceLabel), target.Pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return base.Valid(target, throwMessages);
        }
    }
}

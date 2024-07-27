using System.Collections.Generic;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_ConvertResource : CompAbilityEffect
    {
        public new CompProperties_AbilityConvertResource Props => (CompProperties_AbilityConvertResource)props;
        public List<AbilityComp> comps;

        private bool HasEnoughResource
        {
            get
            {
                ResourceGene gene_Resource = (ResourceGene)parent.pawn.genes.GetGene(Props.giver);
                if (gene_Resource == null || gene_Resource.Value < Props.resourceCost)
                {
                    return false;
                }
                gene_Resource = (ResourceGene)parent.pawn.genes.GetGene(Props.receiver);
                if (gene_Resource == null)
                {
                    return false;
                }
                return true;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            ResourceGene giverGene;
            ResourceGene receiverGene;
            if (Props.giver == null) Log.Error(parent.def.defName + " is missing a designated giver, meaning it can't alter the resource levels");
            else if (Props.receiver == null) Log.Error(parent.def.defName + " is missing a designated receiver, meaning it can't alter the resource levels");
            else
            {
                giverGene = (ResourceGene)pawn.genes.GetGene(Props.giver);
                ResourceGene.OffsetResource(pawn, 0f - Props.resourceCost, giverGene, giverGene.def.GetModExtension<DRGExtension>());
                receiverGene = (ResourceGene)pawn.genes.GetGene(Props.receiver);
                ResourceGene.OffsetResource(pawn, Props.resourceCost * Props.conversionEfficiency, receiverGene, receiverGene.def.GetModExtension<DRGExtension>());
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!EBSGUtilities.HasRelatedGene(parent.pawn, Props.giver))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.giver.LabelCap);
                return true;
            }
            if (!EBSGUtilities.HasRelatedGene(parent.pawn, Props.receiver))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.receiver.LabelCap);
                return true;
            }
            ResourceGene gene_Resource = (ResourceGene)parent.pawn.genes.GetGene(Props.giver);

            if (gene_Resource.Value < Props.resourceCost)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }
            float num = TotalResourceCostOfQueuedAbilities();
            float num2 = Props.resourceCost + num;
            if (Props.resourceCost > float.Epsilon && num2 > gene_Resource.Value)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }
            reason = null;
            return false;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return HasEnoughResource;
        }

        private float TotalResourceCostOfQueuedAbilities()
        {
            float num = ((!(parent.pawn.jobs?.curJob?.verbToUse is Verb_CastAbility verb_CastAbility)) ? 0f : ResourceCost(verb_CastAbility));
            if (parent.pawn.jobs != null)
            {
                for (int i = 0; i < parent.pawn.jobs.jobQueue.Count; i++)
                {
                    if (parent.pawn.jobs.jobQueue[i].job.verbToUse is Verb_CastAbility verb_CastAbility2)
                    {
                        num += ResourceCost(verb_CastAbility2);
                    }
                }
            }
            return num;
        }

        public float ResourceCost(Verb_CastAbility verb_CastAbility2)
        {
            if (comps != null)
            {
                foreach (AbilityComp comp in verb_CastAbility2.ability?.comps)
                {
                    if (comp is CompAbilityEffect_ResourceCost compAbilityEffect_ResourceCost)
                    {
                        return compAbilityEffect_ResourceCost.Props.resourceCost;
                    }
                }
            }
            return 0f;
        }
    }
}

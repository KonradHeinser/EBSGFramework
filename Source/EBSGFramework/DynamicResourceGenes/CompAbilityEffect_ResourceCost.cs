using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompAbilityEffect_ResourceCost : CompAbilityEffect
    {
        public new CompProperties_AbilityResourceCost Props => (CompProperties_AbilityResourceCost)props;
        public List<AbilityComp> comps;

        private bool HasEnoughResource
        {
            get
            {
                ResourceGene gene_Resource = (ResourceGene)parent.pawn.genes.GetGene(Props.mainResourceGene);
                float cost = Props.resourceCost;
                if (Props.costFactorStat != null) cost *= parent.pawn.GetStatValue(Props.costFactorStat);
                if (gene_Resource == null || gene_Resource.Value < cost)
                {
                    return false;
                }
                if (Props.checkMaximum && gene_Resource.Value + cost > gene_Resource.Max)
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
            ResourceGene resourceGene;
            if (Props.mainResourceGene == null) Log.Error("A casted ability is missing a designated mainResourceGene, meaning it can't alter the resource levels");
            else
            {
                resourceGene = (ResourceGene)pawn.genes.GetGene(Props.mainResourceGene);
                float cost = Props.resourceCost;
                if (Props.costFactorStat != null) cost *= parent.pawn.GetStatValue(Props.costFactorStat);
                ResourceGene.OffsetResource(pawn, 0f - cost, resourceGene, resourceGene.def.GetModExtension<DRGExtension>(), storeLimitPassing: !Props.checkMaximum);
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.genes.HasGene(Props.mainResourceGene))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }
            ResourceGene gene_Resource = (ResourceGene)parent.pawn.genes.GetGene(Props.mainResourceGene);
            float cost = Props.resourceCost;
            if (Props.costFactorStat != null) cost *= parent.pawn.GetStatValue(Props.costFactorStat);

            if (gene_Resource.Value < cost)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }
            if (Props.checkMaximum && gene_Resource.Value + cost > gene_Resource.Max)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }
            float num = TotalResourceCostOfQueuedAbilities();
            float num2 = cost + num;
            if (cost > float.Epsilon && num2 > gene_Resource.Value)
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
                        float cost = Props.resourceCost;
                        if (Props.costFactorStat != null) cost *= parent.pawn.GetStatValue(Props.costFactorStat);
                        return cost;
                    }
                }
            }
            return 0f;
        }
    }
}

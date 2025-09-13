using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_ResourceCost : CompAbilityEffect
    {
        public new CompProperties_AbilityResourceCost Props => (CompProperties_AbilityResourceCost)props;

        protected bool HasEnoughResource
        {
            get
            {
                ResourceGene gene_Resource = (ResourceGene)parent.pawn.genes.GetGene(Props.mainResourceGene);
                float cost = Props.resourceCost;
                if (Props.costFactorStat != null) cost *= parent.pawn.StatOrOne(Props.costFactorStat);
                if (gene_Resource == null || gene_Resource.Value < cost)
                {
                    return false;
                }
                if (Props.checkMaximum && gene_Resource.Value + cost > gene_Resource.Max && cost < 0)
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
            if (Props.mainResourceGene == null) 
                Log.Error("A casted ability is missing a designated mainResourceGene, meaning it can't alter the resource levels");
            else if (pawn.genes.GetGene(Props.mainResourceGene) is ResourceGene resourceGene)
            {
                float cost = Props.resourceCost;
                if (Props.costFactorStat != null) cost *= pawn.StatOrOne(Props.costFactorStat);
                ResourceGene.OffsetResource(pawn, 0f - cost, resourceGene, resourceGene.def.GetModExtension<DRGExtension>(), storeLimitPassing: !Props.checkMaximum);
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.HasRelatedGene(Props.mainResourceGene))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }

            if (!(parent.pawn.genes.GetGene(Props.mainResourceGene) is ResourceGene gene_Resource))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }

            float cost = Props.resourceCost;
            if (Props.costFactorStat != null) cost *= parent.pawn.StatOrOne(Props.costFactorStat);

            if (gene_Resource.Value < cost)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }
            if (Props.checkMaximum && gene_Resource.Value + cost > gene_Resource.Max && cost < 0)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, gene_Resource.ResourceLabel);
                return true;
            }
            float num = TotalResourceCostOfQueuedAbilities(parent.pawn, Props.mainResourceGene);
            float num2 = cost + num;
            if (cost > 0 && num2 > gene_Resource.Value)
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

        public static float TotalResourceCostOfQueuedAbilities(Pawn pawn, GeneDef gene)
        {
            float num = (!(pawn.jobs?.curJob?.verbToUse is Verb_CastAbility verb_CastAbility)) ? 0f : ResourceCost(verb_CastAbility, pawn, gene);
            if (pawn.jobs != null)
            {
                for (int i = 0; i < pawn.jobs.jobQueue.Count; i++)
                {
                    if (pawn.jobs.jobQueue[i].job.verbToUse is Verb_CastAbility verb_CastAbility2)
                    {
                        num += ResourceCost(verb_CastAbility2, pawn, gene);
                    }
                }
            }
            return num;
        }

        protected static float ResourceCost(Verb_CastAbility verb_CastAbility2, Pawn pawn, GeneDef gene)
        {
            foreach (AbilityComp comp in verb_CastAbility2.ability?.comps)
            {
                if (comp is CompAbilityEffect_ResourceCost compAbilityEffect_ResourceCost)
                {
                    var p = compAbilityEffect_ResourceCost.Props;
                    if (p.mainResourceGene != gene)
                        continue;
                    float cost = p.resourceCost;
                    if (p.costFactorStat != null) cost *= pawn.StatOrOne(p.costFactorStat);
                    return cost;
                }
            }
            return 0f;
        }
    }
}

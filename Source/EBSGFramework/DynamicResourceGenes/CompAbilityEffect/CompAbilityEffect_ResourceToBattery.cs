using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_ResourceToBattery : CompAbilityEffect
    {
        public new CompProperties_AbilityResourceToBattery Props => (CompProperties_AbilityResourceToBattery)props;
        ResourceGene cachedResourceGene;

        private ResourceGene ResourceGene
        {
            get
            {
                if (cachedResourceGene == null)
                {
                    Gene gene = parent.pawn.genes.GetGene(Props.mainResourceGene);
                    if (gene is ResourceGene resourceGene)
                    {
                        cachedResourceGene = resourceGene;
                    }
                }
                return cachedResourceGene;
            }
        }

        public float MaxCost
        {
            get
            {
                if (ResourceGene == null) return 0;

                float maxCost = Props.maxConverted;

                if (Props.maxPercentOfMax > 0 && ResourceGene.Max * Props.maxPercentOfMax < maxCost) maxCost = ResourceGene.Max * Props.maxPercentOfMax;
                if (Props.maxPercent > 0 && ResourceGene.Value * Props.maxPercent < maxCost) maxCost = ResourceGene.Value * Props.maxPercent;
                if (maxCost > ResourceGene.Value) maxCost = ResourceGene.Value;

                return maxCost;
            }
        }

        private float MaxGain
        {
            get
            {
                float gain = Props.conversionEfficiency * MaxCost * 100;
                if (Props.efficiencyFactorStat != null) gain *= parent.pawn.GetStatValue(Props.efficiencyFactorStat);
                return gain;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing is Building building)
            {
                CompPowerBattery battery = building.GetComp<CompPowerBattery>();
                if (battery != null)
                {
                    float missingEnergy = battery.AmountCanAccept;
                    if (missingEnergy > MaxGain)
                    {
                        battery.AddEnergy(MaxGain);
                        ResourceGene.OffsetResource(parent.pawn, 0 - MaxCost, ResourceGene);
                    }
                    else
                    {
                        battery.AddEnergy(missingEnergy);
                        float offset = missingEnergy / Props.conversionEfficiency;
                        offset /= 100;
                        offset /= battery.Props.efficiency;
                        if (Props.efficiencyFactorStat != null) offset /= parent.pawn.GetStatValue(Props.efficiencyFactorStat);
                        ResourceGene.OffsetResource(parent.pawn, 0 - offset, ResourceGene);
                    }
                }
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.genes.HasGene(Props.mainResourceGene))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }

            if (!(parent.pawn.genes.GetGene(Props.mainResourceGene) is ResourceGene resourceGene))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }

            reason = null;
            return false;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages)) return false;

            if (!target.HasThing) return false;
            if (target.Thing is Building building)
            {
                CompPowerBattery battery = building.GetComp<CompPowerBattery>();
                if (battery != null)
                {
                    return true;
                }
            }
            if (throwMessages)
            {
                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNotBattery".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
            }
            return false;
        }
    }
}

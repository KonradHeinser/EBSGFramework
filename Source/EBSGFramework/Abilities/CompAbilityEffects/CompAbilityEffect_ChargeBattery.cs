using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_ChargeBattery : CompAbilityEffect
    {
        public new CompProperties_AbilityChargeBattery Props => (CompProperties_AbilityChargeBattery)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing is Building building)
            {
                CompPowerBattery battery = building.GetComp<CompPowerBattery>();
                if (battery != null)
                {
                    float charge = Props.batteryChangeAmount;
                    if (Props.efficiencyFactorStat != null) charge *= parent.pawn.StatOrOne(Props.efficiencyFactorStat);
                    battery.AddEnergy(charge);
                }
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
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
                    float charge = Props.batteryChangeAmount;
                    if (Props.efficiencyFactorStat != null) charge *= parent.pawn.StatOrOne(Props.efficiencyFactorStat);

                    if (charge < 0 && battery.StoredEnergy + (charge * battery.Props.efficiency) < 0)
                    {
                        if (throwMessages)
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityLowBattery".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                        return false;
                    }
                    if (!Props.allowOvercharge && battery.StoredEnergy + (charge * battery.Props.efficiency) > battery.Props.storedEnergyMax)
                    {
                        if (throwMessages)
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityHighBattery".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                        return false;
                    }
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

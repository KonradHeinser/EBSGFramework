using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompEquippableAbility : ThingComp
    {
        CompProperties_EquippableAbility Props => (CompProperties_EquippableAbility)props;

        private Pawn Holder => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn ?? (parent?.ParentHolder as Pawn_ApparelTracker)?.pawn;

        private int remainingCharges;

        private int cooldownLeft = 0;

        public override void Notify_Equipped(Pawn pawn)
        {
            Ability ability = AbilityUtility.MakeAbility(Props.ability, Holder);
            ability.RemainingCharges = remainingCharges;
            if (cooldownLeft != 0 && Props.saveCooldown)
                ability.StartCooldown(cooldownLeft);
            Holder.abilities.abilities.Add(ability);
            Holder.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            Ability ability = pawn.abilities.GetAbility(Props.ability);
            if (ability != null)
            {
                cooldownLeft = ability.CooldownTicksRemaining;
                remainingCharges = ability.RemainingCharges;
                pawn.abilities.RemoveAbility(Props.ability);
            }

        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (Holder != null)
                Holder.abilities.RemoveAbility(Props.ability);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingCharges, "remainingCharges", -1);
            Scribe_Values.Look(ref cooldownLeft, "cooldownLeft", 0);
        }
    }
}

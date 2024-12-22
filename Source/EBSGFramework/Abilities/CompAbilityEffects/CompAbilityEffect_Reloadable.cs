using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_Reloadable : CompAbilityEffect
    {
        public new CompProperties_AbilityReloadable Props => (CompProperties_AbilityReloadable)props;

        private int remainingCharges;

        public int RemainingCharges
        {
            get
            {
                return remainingCharges;
            }
            set
            {
                remainingCharges = value;
            }
        }

        public int ChargesNeeded
        {
            get
            {
                return Props.maxCharges - RemainingCharges;
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            if (!base.GizmoDisabled(out reason))
                return base.GizmoDisabled(out reason);

            if (RemainingCharges == 0)
            {
                reason = EBSGUtilities.TranslateOrLiteral(Props.noChargesRemaining, Props.ammoDef.label, parent.def.label);
                return true;
            }

            return false;
        }

        public override string CompInspectStringExtra()
        {
            return EBSGUtilities.TranslateOrLiteral(Props.remainingCharges) + " : " + RemainingCharges.ToString();
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            RemainingCharges--;

            if (Props.removeOnceEmpty)
                parent.pawn.abilities.RemoveAbility(parent.def);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingCharges, "remainingCharges", Props.maxCharges);
        }
    }
}

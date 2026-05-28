using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_ChangeTraits : CompAbilityEffect
    {
        public new CompProperties_AbilityChangeTraits Props => (CompProperties_AbilityChangeTraits)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) && Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            var pawn = Props.onlySelf ? parent.pawn : target.Pawn;

            if (pawn?.story?.traits == null)
                return false;
            
            string baseExplanation = "CannotUseAbility".Translate(parent.def.label) + ": ";

            if (Props.haveNoneOfAddedTraits && pawn.PawnHasAnyOfTraits(out _, null, Props.addedTraits))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + "EBSG_TraitRestrictedEquipment_AnyOne".Translate(), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            if (Props.haveAllOfRemovedTraits && !pawn.PawnHasAllOfTraits(null, Props.removedTraits))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + "EBSG_TraitRestrictedEquipment_All".Translate(), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            if (Props.haveAnyOfRemovedTraits && !pawn.PawnHasAnyOfTraits(out _, null, Props.removedTraits))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + "EBSG_TraitRestrictedEquipment_AnyOne".Translate(), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            return base.Valid(target, throwMessages);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!Props.onlySelf && target.TargetIsPawn(out var pawn))
            {
                pawn.RemoveTraits(null, Props.removedTraits);
                pawn.AddTraits(null, Props.addedTraits);
            }

            if (Props.applyOnSelf || Props.onlySelf)
            {
                parent.pawn.RemoveTraits(null, Props.removedTraits);
                parent.pawn.AddTraits(null, Props.addedTraits);
            }
        }
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (Props.successChance?.hideChance == false && target.Thing != null)
                return "EBSG_SuccessChance".Translate(Math.Round(Props.successChance.Chance(parent.pawn, target.Thing == parent.pawn ? null : target.Thing) * 100, 3));

            return null;
        }
    }
}
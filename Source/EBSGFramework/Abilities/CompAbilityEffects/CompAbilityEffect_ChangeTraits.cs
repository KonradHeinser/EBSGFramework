using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
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
            if (!target.TargetIsPawn(out var pawn)) 
                return false;
            
            if (pawn.story?.traits == null)
                return false;

            if (Props.haveNoneOfAddedTraits && pawn.PawnHasAnyOfTraits(out _, null, Props.addedTraits))
                return false;

            if (Props.haveAllOfRemovedTraits && !pawn.PawnHasAllOfTraits(null, Props.removedTraits))
                return false;

            if (Props.haveAnyOfRemovedTraits && !pawn.PawnHasAnyOfTraits(out _, null, Props.removedTraits))
                return false;
            
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
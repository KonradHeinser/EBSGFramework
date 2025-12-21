using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_PushTarget : CompAbilityEffect_WithDest
    {
        public new CompProperties_AbilityPushTarget Props => (CompProperties_AbilityPushTarget)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.successChance?.Success(parent.pawn, target.Thing == parent.pawn ? null : target.Thing) == false)
                return;

            base.Apply(target, dest);
            LocalTargetInfo destination = GetDestination(dest.IsValid ? dest : target);
            if (!destination.IsValid)
            {
                return;
            }

            Pawn pawn = parent.pawn;

            target.Thing.TryGetComp<CompCanBeDormant>()?.WakeUp();

            if (target.Thing is Pawn pawn2)
            {
                IntVec3 position = pawn2.Position;
                IntVec3 cell = destination.Cell;
                Map map = pawn.Map;
                bool flag = Find.Selector.IsSelected(pawn);
                PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, pawn2, cell, Props.flightEffecterDef, Props.soundLanding, Props.flyWithCarriedThing, null,
                    null, destination);
                if (pawnFlyer != null)
                {
                    FleckMaker.ThrowDustPuff(position.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map, 2f);
                    GenSpawn.Spawn(pawnFlyer, cell, map);
                    if (flag)
                    {
                        Find.Selector.Select(pawn, false, false);
                    }
                }
            }
        }

        public override bool CanHitTarget(LocalTargetInfo target)
        {
            if (!CanPlaceSelectedTargetAt(target))
            {
                return false;
            }
            return base.CanHitTarget(target);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Pawn == null) return false;
            AcceptanceReport acceptanceReport = CanMoveTarget(target);
            if (!acceptanceReport)
            {
                if (showMessages && !acceptanceReport.Reason.NullOrEmpty() && target.Thing is Pawn pawn)
                {
                    Messages.Message("CannotSkipTarget".Translate(pawn.Named("PAWN")) + ": " + acceptanceReport.Reason, pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            return base.Valid(target, showMessages);
        }

        private AcceptanceReport CanMoveTarget(LocalTargetInfo target)
        {
            if (target.Thing is Pawn pawn)
            {
                if (pawn.BodySize > Props.maxBodySize)
                {
                    return "CannotSkipTargetTooLarge".Translate();
                }
            }
            return true;
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            AcceptanceReport report = CanMoveTarget(target);
            if (!report.Accepted)
                return CanMoveTarget(target).Reason;
            if (Props.successChance?.hideChance == false && target.Thing != null)
                return "EBSG_SuccessChance".Translate(Math.Round(Props.successChance.Chance(parent.pawn, target.Thing == parent.pawn ? null : target.Thing) * 100, 3));
            return null;
        }
    }
}

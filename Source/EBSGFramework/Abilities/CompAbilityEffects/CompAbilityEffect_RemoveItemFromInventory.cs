using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_RemoveItemFromInventory : CompAbilityEffect
    {
        public new CompProperties_RemoveItemFromInventory Props => (CompProperties_RemoveItemFromInventory)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.targetThing != null && EBSGUtilities.TargetIsPawn(target, out Pawn targetPawn) && targetPawn.inventory != null)
                if (targetPawn.inventory.innerContainer.Contains(Props.targetThing, Props.targetCount))
                    targetPawn.inventory.RemoveCount(Props.targetThing, Props.targetCount);
                else if (targetPawn.inventory.Count(Props.targetThing) > 0)
                    targetPawn.inventory.RemoveCount(Props.targetThing, targetPawn.inventory.Count(Props.targetThing));

            if (Props.casterThing != null && parent.pawn.inventory != null)
                if (parent.pawn.inventory.innerContainer.Contains(Props.casterThing, Props.casterCount))
                    parent.pawn.inventory.RemoveCount(Props.casterThing, Props.casterCount);
                else if (parent.pawn.inventory.Count(Props.casterThing) > 0)
                    parent.pawn.inventory.RemoveCount(Props.casterThing, parent.pawn.inventory.Count(Props.casterThing));
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (Props.disableGizmoIfCasterInvalid)
                if (!Props.casterBestEffort && Props.casterThing != null)
                    if (parent.pawn.inventory == null || !parent.pawn.inventory.innerContainer.Contains(Props.casterThing, Props.casterCount))
                    {
                        reason = "AbilityCasterInventory".Translate();
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
            string baseExplanation = "CannotUseAbility".Translate(parent.def.label) + ": ";

            if (!Props.targetBestEffort && Props.targetThing != null)
                if (!EBSGUtilities.TargetIsPawn(target, out Pawn targetPawn) || targetPawn.inventory == null || !targetPawn.inventory.innerContainer.Contains(Props.targetThing, Props.targetCount))
                {
                    Map map = targetPawn.Map;
                    if (map == null) map = targetPawn.MapHeld;

                    if (throwMessages)
                        Messages.Message(baseExplanation + "AbilityTargetInventory".Translate(), target.ToTargetInfo(map), MessageTypeDefOf.RejectInput, false);
                    return false;
                }
            if (!Props.casterBestEffort && Props.casterThing != null)
                if (parent.pawn.inventory == null || !parent.pawn.inventory.innerContainer.Contains(Props.casterThing, Props.casterCount))
                {
                    Map map = parent.pawn.Map;
                    if (map == null) map = parent.pawn.MapHeld;

                    if (throwMessages)
                        Messages.Message(baseExplanation + "AbilityCasterInventory".Translate(), target.ToTargetInfo(map), MessageTypeDefOf.RejectInput, false);
                    return false;
                }

            return true;
        }
    }
}

using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_AddItemToInventory : CompAbilityEffect
    {
        public new CompProperties_AddItemToInventory Props => (CompProperties_AddItemToInventory)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.targetThing != null && EBSGUtilities.TargetIsPawn(target, out Pawn targetPawn))
            {
                Thing thing = ThingMaker.MakeThing(Props.targetThing, Props.targetStuffing);
                thing.stackCount = Props.targetCount;

                if (targetPawn.inventory != null && targetPawn.inventory.innerContainer.CanAcceptAnyOf(thing))
                    targetPawn.inventory.innerContainer.TryAdd(thing, Props.targetCount);
            }

            if (Props.casterThing != null && parent.pawn.inventory != null)
            {
                Thing thing = ThingMaker.MakeThing(Props.casterThing, Props.casterStuffing);
                thing.stackCount = Props.casterCount;
                if (parent.pawn.inventory.innerContainer.CanAcceptAnyOf(thing))
                    parent.pawn.inventory.innerContainer.TryAdd(thing, Props.casterCount);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobDriver_DRGFeedPatient : JobDriver
    {
        protected Thing Item => job.targetA.Thing;

        protected Pawn Deliveree => job.targetB.Pawn;

        protected Pawn_InventoryTracker ItemHolderInventory => Item?.ParentHolder as Pawn_InventoryTracker;

        protected Pawn ItemHolder => job.targetC.Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Item == null || Item.TryGetComp<Comp_DRGConsumable>() == null)
                return false;
            int numberToTake = Math.Min(Item.TryGetComp<Comp_DRGConsumable>().NumberToConsume(Deliveree), job.count);
            if (!pawn.Reserve(Deliveree, job, 1, numberToTake, null, errorOnFailed))
                return false;

            if (pawn.inventory == null || !pawn.inventory.Contains(base.TargetThingA))
            {
                if (!pawn.Reserve(Item, job, 10, numberToTake, null, errorOnFailed))
                    return false;

                job.count = numberToTake;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOn(() => !FoodUtility.ShouldBeFedBySomeone(Deliveree));
            Toil carryItemFromInventory = Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
            Toil goToItemHolder = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch).FailOn(() => ItemHolder != ItemHolderInventory?.pawn || ItemHolder.IsForbidden(pawn));
            Toil carryItemToPatient = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_Jump.JumpIf(carryItemFromInventory, () => pawn.inventory != null && pawn.inventory.Contains(base.TargetThingA));
            yield return Toils_Haul.CheckItemCarriedByOtherPawn(Item, TargetIndex.C, goToItemHolder);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
            yield return Toils_DRGConsume.PickupConsumable(TargetIndex.A, Deliveree);
            yield return Toils_Jump.Jump(carryItemToPatient);
            yield return goToItemHolder;
            yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.C);
            yield return Toils_Haul.TakeFromOtherInventory(Item, pawn.inventory.innerContainer, ItemHolderInventory?.innerContainer, job.count, TargetIndex.A);
            yield return carryItemFromInventory;
            yield return Toils_Jump.Jump(carryItemToPatient);
            yield return carryItemToPatient;
            yield return Toils_DRGConsume.ConsumeConsumable(Deliveree, pawn, TargetIndex.A).FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            Toil toil = Toils_DRGConsume.FinalizeConsume(Deliveree, TargetIndex.A);
            yield return toil;
        }
    }
}

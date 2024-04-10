using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using System;

namespace EBSGFramework
{
    public class JobDriver_DRGDeliver : JobDriver
    {
        private bool consumingFromInventory;

        private Pawn Deliveree => (Pawn)job.targetB.Thing;

        protected Thing Item => job.targetA.Thing;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref consumingFromInventory, "consumingFromInventory", false);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            consumingFromInventory = pawn.inventory != null && pawn.inventory.Contains(base.TargetThingA);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Item == null || Item.TryGetComp<Comp_DRGConsumable>() == null)
                return false;
            int numberToTake = Math.Min(Item.TryGetComp<Comp_DRGConsumable>().NumberToConsume(Deliveree), job.count);
            return pawn.Reserve(Deliveree, job, 1, numberToTake, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.B);
            if (consumingFromInventory)
                yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
            else
            {
                yield return Toils_DRGConsume.ReserveFromStackForConsuming(TargetIndex.A, Deliveree);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
                yield return Toils_DRGConsume.PickupConsumable(TargetIndex.A, Deliveree);
            }
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                actor.pather.StartPath(curJob.targetC, PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            toil.AddFailCondition(delegate
            {
                if (!base.pawn.IsCarryingThing(job.GetTarget(TargetIndex.A).Thing))
                    return true;

                Pawn pawn = (Pawn)toil.actor.jobs.curJob.targetB.Thing;
                if (!pawn.IsPrisonerOfColony)
                    return true;

                return !pawn.guest.CanBeBroughtFood;
            });
            yield return toil;
            Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
            toil2.initAction = delegate
            {
                pawn.carryTracker.TryDropCarriedThing(toil2.actor.jobs.curJob.targetC.Cell, ThingPlaceMode.Direct, out var _);
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
        }
    }
}

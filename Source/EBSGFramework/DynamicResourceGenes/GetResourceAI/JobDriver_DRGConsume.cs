using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using System;

namespace EBSGFramework
{
    public class JobDriver_DRGConsume : JobDriver
    {
        private bool consumingFromInventory;

        public const TargetIndex ConsumableSourceInd = TargetIndex.A;

        private const TargetIndex TableCellInd = TargetIndex.B;

        private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

        public bool EatingFromInventory => consumingFromInventory;

        private Thing ConsumableSource => job.GetTarget(TargetIndex.A).Thing;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref consumingFromInventory, "consumingFromInventory", false);
        }

        public override string GetReport()
        {
            Thing thing = job.targetA.Thing;

            if (thing == null)
                return base.GetReport();

            if (thing.HasComp<Comp_DRGConsumable>())
            {
                GeneLinker relatedLinker = thing.TryGetComp<Comp_DRGConsumable>().GetRelatedLinker(pawn);
                if (relatedLinker == null)
                    return base.GetReport();
                if (relatedLinker.consumptionReportString != null)
                    return relatedLinker.consumptionReportString.Formatted(thing.LabelShort, thing);

                foreach (GeneLinker linker in thing.TryGetComp<Comp_DRGConsumable>().Props.resourceOffsets)
                    if (linker.consumptionReportString != null && pawn.HasRelatedGene(linker.mainResourceGene))
                        return linker.consumptionReportString.Formatted(thing.LabelShort, thing);
            }
            return "DRG_Consuming".Translate(thing.LabelShort);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            consumingFromInventory = pawn.inventory != null && pawn.inventory.Contains(ConsumableSource);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Faction != null)
            {
                int maxAmountToPickup = Math.Min(ConsumableSource.TryGetComp<Comp_DRGConsumable>().NumberToConsume(pawn), job.count);
                if (!pawn.Reserve(ConsumableSource, job, 1, maxAmountToPickup, null, errorOnFailed))
                    return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil consume = Toils_DRGConsume.ConsumeConsumable(pawn, pawn, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !ConsumableSource.Spawned && (pawn.carryTracker == null || pawn.carryTracker.CarriedThing != ConsumableSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            foreach (Toil item in PrepareToIngestToils(consume))
                yield return item;
            yield return consume;
            yield return Toils_DRGConsume.FinalizeConsume(pawn, TargetIndex.A);
        }

        private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
        {
            if (pawn.RaceProps.ToolUser && !pawn.IsMutant)
                return PrepareToIngestToils_ToolUser(chewToil);
            return PrepareToConsumableToils_NonToolUser();
        }

        private IEnumerable<Toil> PrepareToIngestToils_ToolUser(Toil chewToil)
        {
            if (consumingFromInventory)
                yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
            else
            {
                yield return ReserveItem();
                Toil gotoToPickup = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
                yield return Toils_Jump.JumpIf(gotoToPickup, () => pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
                yield return Toils_Jump.Jump(chewToil);
                yield return gotoToPickup;
                yield return Toils_DRGConsume.PickupConsumable(TargetIndex.A, pawn);
            }
            if (job.takeExtraIngestibles > 0)
            {
                foreach (Toil item in TakeExtraConsumables())
                {
                    yield return item;
                }
            }
            yield return Toils_DRGConsume.FindAdjacentConsumeSurface(TargetIndex.B, TargetIndex.A);
        }

        private IEnumerable<Toil> PrepareToConsumableToils_NonToolUser()
        {
            yield return ReserveItem();
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        }

        private IEnumerable<Toil> TakeExtraConsumables()
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                yield break;

            Toil reserveExtraItemToCollect = Toils_DRGConsume.ReserveFromStackForConsuming(TargetIndex.C);
            Toil findExtraItemToCollect = ToilMaker.MakeToil("TakeExtraConsumables");
            findExtraItemToCollect.initAction = delegate
            {
                if (pawn.inventory.innerContainer.TotalStackCountOfDef(ConsumableSource.def) < job.takeExtraIngestibles)
                {
                    Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ConsumableSource.def), PathEndMode.Touch, TraverseParms.For(pawn), 30f, (Thing x) => pawn.CanReserve(x, 10, 1) && !x.IsForbidden(pawn) && x.IsSociallyProper(pawn));
                    if (thing != null)
                    {
                        job.SetTarget(TargetIndex.C, thing);
                        JumpToToil(reserveExtraItemToCollect);
                    }
                }
            };
            findExtraItemToCollect.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return Toils_Jump.Jump(findExtraItemToCollect);
            yield return reserveExtraItemToCollect;
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Haul.TakeToInventory(TargetIndex.C, () => job.takeExtraIngestibles - pawn.inventory.innerContainer.TotalStackCountOfDef(ConsumableSource.def));
            yield return findExtraItemToCollect;
        }

        private Toil ReserveItem()
        {
            Toil toil = ToilMaker.MakeToil("ReserveItem");
            toil.initAction = delegate
            {
                if (pawn.Faction != null)
                {
                    Thing thing = job.GetTarget(TargetIndex.A).Thing;
                    if (pawn.carryTracker.CarriedThing != thing)
                    {
                        int maxAmountToPickup = Math.Min(thing.TryGetComp<Comp_DRGConsumable>().NumberToConsume(pawn), job.count);
                        if (maxAmountToPickup != 0)
                        {
                            if (!pawn.Reserve(thing, job, 1, maxAmountToPickup))
                            {
                                Log.Error(string.Concat("Pawn reservation for ", pawn, " on job ", this, " failed, because it could not register from ", thing, " - amount: ", maxAmountToPickup));
                                pawn.jobs.EndCurrentJob(JobCondition.Errored);
                            }
                            job.count = maxAmountToPickup;
                        }
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.atomicWithPrevious = true;
            return toil;
        }

        public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool flip)
        {
            IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
            return ModifyCarriedThingDrawPosWorker(ref drawPos, ref flip, cell, pawn);
        }

        public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool flip, IntVec3 placeCell, Pawn pawn)
        {
            if (pawn.pather.Moving)
                return false;

            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null || !carriedThing.IsBurning() || !carriedThing.HasComp<Comp_DRGConsumable>())
                return false;

            GeneLinker relatedLinker = carriedThing.TryGetComp<Comp_DRGConsumable>()?.GetRelatedLinker(pawn);

            if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map))
            {
                drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
                return true;
            }
            HoldOffset holdOffset = relatedLinker.consumeHoldOffset?.Pick(pawn.Rotation);
            if (holdOffset != null)
            {
                drawPos += holdOffset.offset;
                flip = holdOffset.flip;
                return true;
            }
            return false;
        }
    }
}

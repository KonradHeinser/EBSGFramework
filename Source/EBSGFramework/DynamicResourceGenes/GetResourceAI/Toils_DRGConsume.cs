using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public static class Toils_DRGConsume
    {
        public const int MaxPawnReservations = 10;

        private static List<IntVec3> spotSearchList = new List<IntVec3>();

        private static List<IntVec3> cardinals = GenAdj.CardinalDirections.ToList();

        private static List<IntVec3> diagonals = GenAdj.DiagonalDirections.ToList();

        public static Toil ConsumeConsumable(Pawn consumer, Pawn deliverer, TargetIndex consumeInd, TargetIndex eatSurfaceInd = TargetIndex.None)
        {
            Toil toil = ToilMaker.MakeToil("ConsumeConsumable");
            GeneLinker relatedLinker = deliverer.CurJob.GetTarget(consumeInd).Thing?.TryGetComp<Comp_DRGConsumable>()?.GetRelatedLinker(consumer);
            toil.initAction = delegate
            {
                Thing thing4 = deliverer.CurJob.GetTarget(consumeInd).Thing;

                if (thing4.IsBurning() || !thing4.HasComp<Comp_DRGConsumable>() || relatedLinker == null)
                    consumer.jobs.EndCurrentJob(JobCondition.Incompletable);
                else
                {
                    deliverer.pather.StopDead();
                    deliverer.jobs.curDriver.ticksLeftThisToil = relatedLinker.ticks;
                    if (thing4.Spawned)
                        thing4.Map.physicalInteractionReservationManager.Reserve(consumer, deliverer.CurJob, thing4);
                }
            };
            toil.tickAction = delegate
            {
                if (consumer != deliverer)
                    deliverer.rotationTracker.FaceCell(consumer.Position);
                else
                {
                    Thing thing3 = deliverer.CurJob.GetTarget(consumeInd).Thing;
                    if (thing3 != null && thing3.Spawned)
                        deliverer.rotationTracker.FaceCell(thing3.Position);

                    else if (eatSurfaceInd != 0 && deliverer.CurJob.GetTarget(eatSurfaceInd).IsValid)
                        deliverer.rotationTracker.FaceCell(deliverer.CurJob.GetTarget(eatSurfaceInd).Cell);
                }
                deliverer.GainComfortFromCellIfPossible();
            };
            toil.WithProgressBar(consumeInd, delegate
            {
                Thing thing2 = deliverer.CurJob.GetTarget(consumeInd).Thing;
                return (thing2 == null) ? 1f : (1f - (float)deliverer.jobs.curDriver.ticksLeftThisToil / relatedLinker.ticks);
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(consumeInd);
            toil.AddFinishAction(delegate
            {
                Thing thing = consumer?.CurJob?.GetTarget(consumeInd).Thing;
                if (thing != null && consumer.Map.physicalInteractionReservationManager.IsReservedBy(consumer, thing))
                    consumer.Map.physicalInteractionReservationManager.Release(consumer, deliverer.CurJob, thing);
            });
            toil.handlingFacing = true;
            AddConsumptionEffects(toil, consumer, deliverer, consumeInd, eatSurfaceInd, relatedLinker);
            return toil;
        }

        public static Toil AddConsumptionEffects(Toil toil, Pawn consumer, Pawn deliverer, TargetIndex consumeInd, TargetIndex eatSurfaceInd, GeneLinker relatedLinker)
        {
            toil.WithEffect(delegate
            {
                LocalTargetInfo target2 = deliverer.CurJob.GetTarget(consumeInd);
                if (!target2.HasThing)
                    return null;

                EffecterDef result = null;

                if (relatedLinker.consumeEffect == null)
                    foreach (GeneLinker linker in target2.Thing.TryGetComp<Comp_DRGConsumable>().Props.resourceOffsets)
                        if (linker.consumeEffect != null && consumer.HasRelatedGene(linker.mainResourceGene))
                            result = linker.consumeEffect;
                return result;
            }, delegate
            {
                if (consumer != deliverer)
                    return consumer;

                if (!deliverer.CurJob.GetTarget(consumeInd).HasThing)
                    return null;
                Thing thing = deliverer.CurJob.GetTarget(consumeInd).Thing;

                return (eatSurfaceInd != 0 && deliverer.CurJob.GetTarget(eatSurfaceInd).IsValid) ? deliverer.CurJob.GetTarget(eatSurfaceInd) : (thing);
            });
            if (!consumer.RaceProps.Humanlike || relatedLinker.consumeSound != null)
                toil.PlaySustainerOrSound(delegate
                {
                    if (!consumer.RaceProps.Humanlike)
                        return consumer.RaceProps.soundEating;
                    return relatedLinker.consumeSound;
                });
            return toil;
        }


        public static bool TryFindFreeSittingSpotOnThing(Thing t, Pawn pawn, out IntVec3 cell)
        {
            foreach (IntVec3 item in t.OccupiedRect())
            {
                if (pawn.CanReserveSittableOrSpot(item))
                {
                    cell = item;
                    return true;
                }
            }
            cell = IntVec3.Invalid;
            return false;
        }

        public static Toil FindAdjacentConsumeSurface(TargetIndex eatSurfaceInd, TargetIndex foodInd)
        {
            Toil toil = ToilMaker.MakeToil("FindAdjacentConsumeSurface");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                IntVec3 position = actor.Position;
                Map map = actor.Map;
                for (int i = 0; i < 4; i++)
                {
                    IntVec3 intVec = position + new Rot4(i).FacingCell;
                    if (intVec.HasEatSurface(map))
                    {
                        toil.actor.CurJob.SetTarget(eatSurfaceInd, intVec);
                        toil.actor.jobs.curDriver.rotateToFace = eatSurfaceInd;
                        Thing thing = toil.actor.CurJob.GetTarget(foodInd).Thing;
                        if (thing.def.rotatable)
                        {
                            thing.Rotation = Rot4.FromIntVec3(intVec - toil.actor.Position);
                        }
                        break;
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil PickupConsumable(TargetIndex ind, Pawn eater)
        {
            Toil toil = ToilMaker.MakeToil("PickupIngestible");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(ind).Thing;
                if (curJob.count <= 0)
                {
                    Log.Error("Tried to do PickupConsumable toil with job.count = " + curJob.count);
                    actor.jobs.EndCurrentJob(JobCondition.Errored);
                }
                else
                {
                    int count = thing.stackCount;
                    if (thing.HasComp<Comp_DRGConsumable>())
                        count = Math.Min(count, thing.TryGetComp<Comp_DRGConsumable>().NumberToConsume(eater));
                    else
                        count = Math.Min(count, curJob.count);

                    actor.carryTracker.TryStartCarry(thing, count);
                    if (thing != actor.carryTracker.CarriedThing && actor.Map.reservationManager.ReservedBy(thing, actor, curJob))
                        actor.Map.reservationManager.Release(thing, actor, curJob);

                    actor.jobs.curJob.targetA = actor.carryTracker.CarriedThing;
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil ReserveFromStackForConsuming(TargetIndex ind, Pawn ingester = null)
        {
            Toil toil = ToilMaker.MakeToil("ReserveFromStackForConsuming");
            toil.initAction = delegate
            {
                if (ingester == null)
                    ingester = toil.actor;

                LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(ind);

                int stackCount = -1;

                if (target.HasThing && target.Thing.SpawnedOrAnyParentSpawned && !target.Thing.IsBurning() && target.Thing.HasComp<Comp_DRGConsumable>())
                {
                    int b = target.Thing.TryGetComp<Comp_DRGConsumable>().NumberToConsume(ingester);
                    stackCount = Math.Min(target.Thing.stackCount, b);
                }
                if (!target.HasThing || stackCount == -1 || !toil.actor.CanReserve(target, 10, stackCount))
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);

                toil.actor.Reserve(target, toil.actor.CurJob, 10, stackCount);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.atomicWithPrevious = true;
            return toil;
        }

        public static Toil FinalizeConsume(Pawn ingester, TargetIndex consumeInd)
        {
            Toil toil = ToilMaker.MakeToil("FinalizeConsume");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(consumeInd).Thing;
                CompProperties_DRGConsumable compProps = thing?.TryGetComp<Comp_DRGConsumable>()?.Props;

                if (compProps != null)
                    foreach (GeneLinker linker in compProps.resourceOffsets)
                        if (ingester.HasRelatedGene(linker.mainResourceGene) && ingester.genes.GetGene(linker.mainResourceGene) is ResourceGene resource)
                            ResourceGene.OffsetResource(ingester, linker.amount * thing.stackCount, resource, null, true);
                thing.Destroy();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}
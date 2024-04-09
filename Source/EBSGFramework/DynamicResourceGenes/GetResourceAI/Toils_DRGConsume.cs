using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using System;
using UnityEngine;

namespace EBSGFramework
{
    public static class Toils_DRGConsume
    {
        public const int MaxPawnReservations = 10;

        private static List<IntVec3> spotSearchList = new List<IntVec3>();

        private static List<IntVec3> cardinals = GenAdj.CardinalDirections.ToList();

        private static List<IntVec3> diagonals = GenAdj.DiagonalDirections.ToList();

        public static Toil ConsumeConsumable(Pawn consumer, TargetIndex consumeInd, TargetIndex eatSurfaceInd = TargetIndex.None)
        {
            Toil toil = ToilMaker.MakeToil("ConsumeConsumable");
            GeneLinker relatedLinker = toil.actor.CurJob.GetTarget(consumeInd).Thing?.TryGetComp<Comp_DRGConsumable>()?.GetRelatedLinker(toil.actor);
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Thing thing4 = actor.CurJob.GetTarget(consumeInd).Thing;

                if (thing4.IsBurning() || !thing4.HasComp<Comp_DRGConsumable>() || relatedLinker == null)
                    consumer.jobs.EndCurrentJob(JobCondition.Incompletable);
                else
                {
                    toil.actor.pather.StopDead();
                    actor.jobs.curDriver.ticksLeftThisToil = relatedLinker.ticks;
                    if (thing4.Spawned)
                        thing4.Map.physicalInteractionReservationManager.Reserve(consumer, actor.CurJob, thing4);
                }
            };
            toil.tickAction = delegate
            {
                if (consumer != toil.actor)
                    toil.actor.rotationTracker.FaceCell(consumer.Position);
                else
                {
                    Thing thing3 = toil.actor.CurJob.GetTarget(consumeInd).Thing;
                    if (thing3 != null && thing3.Spawned)
                        toil.actor.rotationTracker.FaceCell(thing3.Position);

                    else if (eatSurfaceInd != 0 && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid)
                        toil.actor.rotationTracker.FaceCell(toil.actor.CurJob.GetTarget(eatSurfaceInd).Cell);
                }
                toil.actor.GainComfortFromCellIfPossible();
            };
            toil.WithProgressBar(consumeInd, delegate
            {
                Thing thing2 = toil.actor.CurJob.GetTarget(consumeInd).Thing;
                return (thing2 == null) ? 1f : (1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / relatedLinker.ticks);
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(consumeInd);
            toil.AddFinishAction(delegate
            {
                Thing thing = consumer?.CurJob?.GetTarget(consumeInd).Thing;
                if (thing != null && consumer.Map.physicalInteractionReservationManager.IsReservedBy(consumer, thing))
                    consumer.Map.physicalInteractionReservationManager.Release(consumer, toil.actor.CurJob, thing);
            });
            toil.handlingFacing = true;
            AddConsumptionEffects(toil, consumer, consumeInd, eatSurfaceInd, relatedLinker);
            return toil;
        }

        public static Toil AddConsumptionEffects(Toil toil, Pawn consumer, TargetIndex consumeInd, TargetIndex eatSurfaceInd, GeneLinker relatedLinker)
        {
            toil.WithEffect(delegate
            {
                LocalTargetInfo target2 = toil.actor.CurJob.GetTarget(consumeInd);
                if (!target2.HasThing || relatedLinker.consumeEffect == null)
                    return null;

                EffecterDef result = relatedLinker.consumeEffect;
                return result;
            }, delegate
            {
                if (!toil.actor.CurJob.GetTarget(consumeInd).HasThing)
                    return null;

                Thing thing = toil.actor.CurJob.GetTarget(consumeInd).Thing;
                if (consumer != toil.actor)
                {
                    return consumer;
                }
                return (eatSurfaceInd != 0 && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid) ? toil.actor.CurJob.GetTarget(eatSurfaceInd) : ((LocalTargetInfo)thing);
            });
            toil.PlaySustainerOrSound(delegate
            {
                if (!consumer.RaceProps.Humanlike)
                    return consumer.RaceProps.soundEating;

                LocalTargetInfo target = toil.actor.CurJob.GetTarget(consumeInd);
                return (!target.HasThing || relatedLinker.consumeSound == null) ? null : relatedLinker.consumeSound;
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
                    int count = Mathf.Min(thing.stackCount, curJob.count);
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
                {
                    ingester = toil.actor;
                }
                int stackCount = -1;
                LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(ind);
                if (target.HasThing && target.Thing.SpawnedOrAnyParentSpawned && target.Thing.IsBurning() && target.Thing.HasComp<Comp_DRGConsumable>())
                {
                    int b = target.Thing.TryGetComp<Comp_DRGConsumable>().NumberToConsume(ingester);
                    stackCount = Mathf.Min(target.Thing.stackCount, b);
                }
                if (!target.HasThing || !toil.actor.CanReserve(target, 10, stackCount))
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
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
                        if (EBSGUtilities.HasRelatedGene(actor, linker.mainResourceGene) && actor.genes.GetGene(linker.mainResourceGene) is ResourceGene resource)
                            ResourceGene.OffsetResource(actor, linker.amount * thing.stackCount, resource, null, true);
                thing.Destroy();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}
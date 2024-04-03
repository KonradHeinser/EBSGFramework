using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using System;

namespace EBSGFramework
{
    public class JobDriver_PawnNeedCharger : JobDriver
    {
        private const TargetIndex ChargerInd = TargetIndex.A;

        public Building_PawnNeedCharger NeedCharger => (Building_PawnNeedCharger)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(NeedCharger, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !NeedCharger.PawnCanUse(pawn));
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {
                NeedCharger.StartCharging(pawn);
            };
            toil.AddFinishAction(NeedCharger.StopCharging);
            toil.handlingFacing = true;
            toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
            {
                pawn.rotationTracker.FaceTarget(NeedCharger.Position);
                if (!job.playerForced && NeedCharger.PawnShouldStop(pawn))
                {
                    ReadyForNextToil();
                }
            });
            yield return toil;
        }
    }
}

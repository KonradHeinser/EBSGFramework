using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobDriver_EnterSleepCasket : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil toil = Toils_General.Wait(500);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            yield return toil;
            Toil enter = ToilMaker.MakeToil("MakeNewToils");
            enter.initAction = delegate
            {
                Pawn actor = enter.actor;
                Building_SleepCasket pod = (Building_SleepCasket)actor.CurJob.targetA.Thing;
                Action action = delegate
                {
                    bool flag = actor.DeSpawnOrDeselect();
                    if (pod.TryAcceptThing(actor) && flag)
                        Find.Selector.Select(actor, false, false);
                };
                if (!pod.def.building.isPlayerEjectable)
                {
                    if (Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(actor.Named("PAWN")).AdjustedFor(actor), action));
                    else
                        action();
                }
                else
                    action();
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }
    }
}

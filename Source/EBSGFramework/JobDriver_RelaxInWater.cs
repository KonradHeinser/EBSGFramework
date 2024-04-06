using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class JobDriver_RelaxInWater : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Toil toil = ToilMaker.MakeToil("RelaxInWater");
            toil.initAction = delegate
            {
                pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = job.def.joyDuration;
            toil.FailOn(() => EBSGUtilities.BadWeather(pawn.Map));
            yield return toil;
        }

    }
}

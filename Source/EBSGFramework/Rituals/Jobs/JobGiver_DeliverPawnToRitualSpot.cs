using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class JobGiver_DeliverPawnToRitualSpot : JobGiver_DeliverPawnToCell
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn t = pawn.mindState.duty.focusSecond.Pawn;
            if (t?.Dead != false)
                return null;

            // Make sure they both want to do the same thing
            if (t.GetLord() != pawn.GetLord())
                return null;

            // See if they can walk themselves there
            if (!t.Downed && !t.IsPrisoner)
                return null;

            // Make sure the pawn isn't already there
            LocalTargetInfo d = GetDestination(t);
            if (!d.IsValid || t.Position == d.Cell)
                return null;

            // Make sure the pawn can actually reach the other pawn
            if (!pawn.CanReach(t, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, maxDanger)))
                return null;

            // Haul the target pawn to the ritual spot cell
            Job job = JobMaker.MakeJob(JobDefOf.DeliverToCell, t, d).WithCount(1);
            job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
            job.expiryInterval = jobMaxDuration;
            return job;
        }
    }
}

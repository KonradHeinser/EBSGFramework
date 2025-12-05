using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class CompAbilityEffect_ForceJobOnHostiles : CompAbilityEffect_WithDest
    {
        public new CompProperties_AbilityForceJob Props => (CompProperties_AbilityForceJob)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Thing is Pawn pawn && pawn.HostileTo(parent.pawn))
            {
                Job job = JobMaker.MakeJob(Props.jobDef, new LocalTargetInfo(GetDestination(target).Cell));
                float num = 1f;
                if (Props.durationMultiplier != null)
                {
                    num = pawn.StatOrOne(Props.durationMultiplier, StatRequirement.Always, 60);
                }
                job.expiryInterval = (parent.def.GetStatValueAbstract(StatDefOf.Ability_Duration, parent.pawn) * num).SecondsToTicks();
                job.mote = MoteMaker.MakeThoughtBubble(pawn, parent.def.iconPath, true);
                RestUtility.WakeUp(pawn);
                pawn.jobs.StopAll();
                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            }
        }
    }
}

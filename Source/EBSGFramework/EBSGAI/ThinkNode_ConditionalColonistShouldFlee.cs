using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalColonistShouldFlee : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.Spawned || pawn.Downed) return false; // If you're not even able to flee, then don't try
            if (pawn.GetInvisibilityComp() != null) return false; // If you're invisible, then you should be safe
            if (PawnUtility.PlayerForcedJobNowOrSoon(pawn) || pawn.Drafted) return false; // If there's a forced job, return false
            if (pawn.CurJob != null && (pawn.CurJob.def == JobDefOf.Flee || pawn.CurJob.def == JobDefOf.FleeAndCower)) return true;
            if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse && pawn.playerSettings.hostilityResponse == HostilityResponseMode.Flee)
            {
                // If shot recently, then it's a threat to avoid
                if (pawn.mindState.lastRangedHarmTick > 0 && Find.TickManager.TicksGame < pawn.mindState.lastRangedHarmTick + 2500) return true;

                // If it's in melee range and an active, non-invisible threat, flee
                Pawn meleeThreat = pawn.mindState.meleeThreat;
                if (meleeThreat != null && pawn.mindState.MeleeThreatStillThreat)
                {
                    if (meleeThreat.GetInvisibilityComp() != null && !IsHunting(pawn, meleeThreat) && !IsDueling(pawn, meleeThreat)) return true;
                }
                // If any enemies are nearby, get out of there
                if (PawnUtility.EnemiesAreNearby(pawn, 9, true, 20, 1)) return true;
            }

            return false;
        }

        private bool IsDueling(Pawn pawn, Pawn other)
        {
            if (pawn.GetLord()?.LordJob is LordJob_Ritual_Duel lordJob_Ritual_Duel)
            {
                return lordJob_Ritual_Duel.Opponent(pawn) == other;
            }
            return false;
        }

        private bool IsHunting(Pawn pawn, Pawn prey)
        {
            if (pawn.CurJob == null)
            {
                return false;
            }
            if (pawn.jobs.curDriver is JobDriver_Hunt jobDriver_Hunt)
            {
                return jobDriver_Hunt.Victim == prey;
            }
            if (pawn.jobs.curDriver is JobDriver_PredatorHunt jobDriver_PredatorHunt)
            {
                return jobDriver_PredatorHunt.Prey == prey;
            }
            return false;
        }
    }
}

using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class ThinkNode_RecordPassage : ThinkNode_Conditional
    {
        // This debugger can be added in any subnode (usually at top) to report both passage and various other bits of information
        private string output = "Point A has been passed";
        private bool reportJob = false;
        private bool reportStance = false;
        private bool reportTarget = false;
        private bool autoSearchForTarget = true;
        private bool reportLord = false;
        private bool reportMindState = false;
        private bool reportVerb = false;
        private PawnKindDef requiredPawnKindDef = null;

        protected override bool Satisfied(Pawn pawn)
        {
            if (requiredPawnKindDef != null && pawn.kindDef != requiredPawnKindDef) return false;

            Log.Warning(output + "EBSG_ThinkingPawnName".Translate() + pawn.Label);
            if (reportJob)
            {
                if (pawn.CurJobDef == null) Log.Message("EBSG_NoJobFound".Translate());
                else Log.Message("EBSG_PawnCurrentJob".Translate() + pawn.CurJobDef);
            }
            if (reportStance)
            {
                if (pawn.stances.curStance == null) Log.Message("EBSG_NoStanceFound".Translate());
                else Log.Message("EBSG_PawnCurrentStance".Translate() + pawn.stances.curStance);
            }
            if (reportTarget)
            {
                Thing target = pawn.GetCurrentTarget(true, false, autoSearchForTarget);
                if (target == null) Log.Message("EBSG_NoTargetFound".Translate());
                else Log.Message("EBSG_PawnCurrentTarget".Translate(target.Label, target.Position.DistanceTo(pawn.Position)));
            }
            if (reportLord)
            {
                Lord lord = pawn.GetLord();
                if (lord == null) Log.Message("No lord found.");
                else if (lord.CurLordToil == null) Log.Message("Has a lord, but no toil.");
                else Log.Message($"Currently has a lord toil of {lord.CurLordToil}");
            }
            if (reportMindState)
            {
                string message = "";
                if (pawn.mindState.enemyTarget == null) message += "Mind has no target, ";
                else message += $"Mind target is {pawn.mindState.enemyTarget.Label}, ";
                if (pawn.mindState.meleeThreat == null) message += "doesn't have a melee threat, ";
                else message += $"has a current melee threat of {pawn.mindState.meleeThreat.Label}, ";
                if (pawn.mindState.duty == null) message += "and does not have a duty listed. ";
                else message += $"and has a listed duty of {pawn.mindState.duty}. ";
                Log.Message(message);
            }
            if (reportVerb)
            {
                Thing enemy = pawn.mindState.enemyTarget ?? pawn.GetCurrentTarget(true, false, autoSearchForTarget);
                Verb verb = pawn.TryGetAttackVerb(enemy);
                if (verb == null) Log.Message($"No verb was available to attack {enemy?.Label + " " ?? ""}with");
                else Log.Message($"Attack verb ({enemy?.Label ?? "no target"}) is {verb}");
            }
            return true;
        }
    }
}

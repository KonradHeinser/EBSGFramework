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
                Thing target = pawn.GetCurrentTarget(false, false, autoSearchForTarget);
                if (target == null) Log.Message("EBSG_NoTargetFound".Translate());
                else Log.Message("EBSG_PawnCurrentTarget".Translate(target.Label, target.Position.DistanceTo(pawn.Position)));
            }
            if (reportLord)
            {
                if (pawn.GetLord() == null) Log.Message("No lord found.");
                else Log.Message($"Currently has a lord of {pawn.GetLord().GetType()}");
            }
            return true;
        }
    }
}

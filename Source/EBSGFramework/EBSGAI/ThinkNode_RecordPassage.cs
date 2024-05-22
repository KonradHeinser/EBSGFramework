using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_RecordPassage : ThinkNode_Conditional
    {
        // This debugger can be added in any subnode (usually at top) to report both passage and various other bits of information
        private string output = "Point A has been passed";
        private bool reportJob = false;
        private bool reportStance = false;
        private bool reportTarget = false;
        protected override bool Satisfied(Pawn pawn)
        {
            Log.Error(output + " - thinking pawn's name: " + pawn.Label);
            if (reportJob)
            {
                if (pawn.CurJobDef == null) Log.Message("No job was found");
                else Log.Message("Pawn is currently performing the job: " + pawn.CurJobDef);
            }
            if (reportStance)
            {
                if (pawn.stances.curStance == null) Log.Message("No stance was found");
                else Log.Message("Pawn's current stance is: " + pawn.stances.curStance);
            }
            if (reportTarget)
            {
                Thing target = EBSGUtilities.GetCurrentTarget(pawn, false, false, false);
                if (target == null) Log.Message("Pawn does not currently have a target");
                else Log.Message("Current target is " + target.Label);
            }
            return true;
        }
    }
}

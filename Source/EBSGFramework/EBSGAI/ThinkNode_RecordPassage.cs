﻿using Verse;
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
                Thing target = EBSGUtilities.GetCurrentTarget(pawn, false, false, false);
                if (target == null) Log.Message("EBSG_NoTargetFound".Translate());
                else Log.Message("EBSG_PawnCurrentTarget".Translate() + target.Label);
            }
            return true;
        }
    }
}

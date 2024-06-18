using Verse;
using Verse.AI;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalEBSGSetting : ThinkNode_Conditional
    {
        private string uniqueID = null;
        private string settingID = null;

        protected override bool Satisfied(Pawn pawn)
        {
            if (!EBSG_Settings.NeedEBSGThinkTree())
                return false;

            if (uniqueID != null && settingID != null)
                return EBSG_Settings.FetchThinkTreeSetting(uniqueID, settingID);
            return true;
        }
    }
}

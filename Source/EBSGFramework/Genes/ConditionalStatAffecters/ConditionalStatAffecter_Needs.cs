using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Needs : ConditionalStatAffecter
    {
        public List<NeedLevel> needLevels;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectNeeds".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
                return req.Pawn.AllNeedLevelsMet(needLevels);

            return false;
        }
    }
}

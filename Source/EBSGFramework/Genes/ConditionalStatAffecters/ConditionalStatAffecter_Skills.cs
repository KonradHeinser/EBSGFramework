using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Skills : ConditionalStatAffecter
    {
        public List<SkillLevel> skillLimiters;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrFormat();
            return "EBSG_CorrectSkills".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
                return pawn.AllSkillLevelsMet(skillLimiters);

            return false;
        }
    }
}

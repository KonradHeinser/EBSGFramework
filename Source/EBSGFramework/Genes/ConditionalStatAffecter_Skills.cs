using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Skills : ConditionalStatAffecter
    {
        public List<SkillCheck> skillLimiters;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectSkills".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
            {
                foreach (SkillCheck skillCheck in skillLimiters)
                {
                    SkillRecord skill = req.Pawn.skills.GetSkill(skillCheck.skill);
                    if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                    {
                        if (skillCheck.minLevel > 0)
                            return false;
                        continue;
                    }

                    if (skill.Level < skillCheck.minLevel)
                        return false;
                    if (skill.Level > skillCheck.maxLevel)
                        return false;
                }
                return true;
            }
            return false;
        }
    }
}

using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityLearnSkill : CompProperties_AbilityEffectWithDuration
    {
        public List<SkillXP> skillsToGiveXP;

        public List<SkillXP> casterskillsToGiveXP;

        public List<SkillXP> targetSkillsToGiveXP;

        public bool preventRepeatsForRandoms = true;

        public CompProperties_AbilityLearnSkill()
        {
            compClass = typeof(CompAbilityEffect_LearnSkill);
        }
    }
}

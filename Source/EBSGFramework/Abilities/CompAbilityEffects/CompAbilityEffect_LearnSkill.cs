using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_LearnSkill : CompAbilityEffect
    {
        public new CompProperties_AbilityLearnSkill Props => (CompProperties_AbilityLearnSkill)props;

        public List<SkillRecord> targetSkills = new List<SkillRecord>();

        public List<SkillRecord> casterSkills = new List<SkillRecord>();

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            targetSkills.Clear();
            casterSkills.Clear();
            Pawn targetPawn = target.Pawn;
            Pawn caster = parent.pawn;

            if (!Props.targetSkillsToGiveXP.NullOrEmpty() && targetPawn?.skills != null)
                foreach (SkillXP skillXP in Props.targetSkillsToGiveXP)
                {
                    SkillRecord skill = null;
                    if (skillXP.skill != null) skill = targetPawn.skills.GetSkill(skillXP.skill);
                    else skill = targetPawn.skills.skills.Where((SkillRecord s) => !s.TotallyDisabled && (!Props.preventRepeatsForRandoms || !targetSkills.Contains(s))).RandomElement();
                    if (skill != null && !skill.TotallyDisabled)
                    {
                        targetSkills.Add(skill);
                        targetPawn.skills.Learn(skill.def, skillXP.experience, true);
                    }
                }

            if (!Props.casterskillsToGiveXP.NullOrEmpty() && caster?.skills != null)
                foreach (SkillXP skillXP in Props.casterskillsToGiveXP)
                {
                    SkillRecord skill = null;
                    if (skillXP.skill != null) skill = caster.skills.GetSkill(skillXP.skill);
                    else skill = caster.skills.skills.Where((SkillRecord s) => !s.TotallyDisabled && (!Props.preventRepeatsForRandoms || !casterSkills.Contains(s))).RandomElement();
                    if (skill != null && !skill.TotallyDisabled)
                    {
                        casterSkills.Add(skill);
                        caster.skills.Learn(skill.def, skillXP.experience, true);
                    }
                }

            if (!Props.skillsToGiveXP.NullOrEmpty())
            {
                foreach (SkillXP skillXP in Props.skillsToGiveXP)
                {
                    if (caster?.skills != null)
                    {
                        SkillRecord skill = null;
                        if (skillXP.skill != null) skill = caster.skills.GetSkill(skillXP.skill);
                        else skill = caster.skills.skills.Where((SkillRecord s) => !s.TotallyDisabled && (!Props.preventRepeatsForRandoms || !casterSkills.Contains(s))).RandomElement();
                        if (skill != null && !skill.TotallyDisabled)
                        {
                            casterSkills.Add(skill);
                            caster.skills.Learn(skill.def, skillXP.experience, true);
                        }
                    }
                    if (targetPawn != caster && targetPawn?.skills != null)
                    {
                        SkillRecord skill = null;
                        if (skillXP.skill != null) skill = targetPawn.skills.GetSkill(skillXP.skill);
                        else skill = targetPawn.skills.skills.Where((SkillRecord s) => !s.TotallyDisabled && (!Props.preventRepeatsForRandoms || !targetSkills.Contains(s))).RandomElement();
                        if (skill != null && !skill.TotallyDisabled)
                        {
                            targetSkills.Add(skill);
                            targetPawn.skills.Learn(skill.def, skillXP.experience, true);
                        }
                    }
                }
            }
        }
    }
}

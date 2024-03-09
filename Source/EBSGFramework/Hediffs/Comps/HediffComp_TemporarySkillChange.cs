using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace EBSGFramework
{
    public class HediffComp_TemporarySkillChange : HediffComp
    {
        private HediffCompProperties_TemporarySkillChange Props => (HediffCompProperties_TemporarySkillChange)props;
        public List<SkillDef> changedSkills;
        public List<int> changedAmounts;
        public List<Passion> originalPassions;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (changedSkills == null) changedSkills = new List<SkillDef>();
            if (changedAmounts == null) changedAmounts = new List<int>();
            if (originalPassions == null) originalPassions = new List<Passion>();

            foreach (SkillChange skillChange in Props.skillChanges)
            {
                SkillRecord skill;
                if (skillChange.skill == null)
                {
                    IEnumerable<SkillRecord> validSkills = Pawn.skills.skills.Where((SkillRecord s) => !s.TotallyDisabled && !changedSkills.Contains(s.def) && !Redundant(s, skillChange));
                    if (validSkills.EnumerableNullOrEmpty()) continue;
                    skill = validSkills.RandomElement();
                }
                else skill = Pawn.skills.GetSkill(skillChange.skill);
                if (skill == null) continue;

                if (skill.Level + skillChange.skillChange > 20) changedAmounts.Add(20 - skill.Level);
                else if (skill.Level + skillChange.skillChange < 0) changedAmounts.Add(skill.Level * -1);
                else changedAmounts.Add(skillChange.skillChange);

                skill.Level += skillChange.skillChange;
                originalPassions.Add(skill.passion);

                if (skillChange.setPassion)
                {
                    skill.passion = skillChange.passion;
                }
                else
                {
                    if (skillChange.passionChange < 0)
                    {
                        switch (skill.passion)
                        {
                            case Passion.Major:
                                if (skillChange.passionChange == -1)
                                {
                                    skill.passion = Passion.Minor;
                                }
                                else
                                {
                                    skill.passion = Passion.None;
                                }
                                break;
                            case Passion.Minor:
                                skill.passion = Passion.None;
                                break;
                            default:
                                skill.passion = Passion.None;
                                break;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < skillChange.passionChange; i++)
                        {
                            if (skill.passion == skill.passion.IncrementPassion()) break;
                            skill.passion = skill.passion.IncrementPassion();
                        }
                    }
                }

                changedSkills.Add(skill.def);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            int noSkillCounter = 0;
            int originalCounter = 0;
            if (changedSkills == null) changedSkills = new List<SkillDef>();
            if (changedAmounts == null) changedAmounts = new List<int>();
            if (originalPassions == null) originalPassions = new List<Passion>();

            if (changedSkills.Count != changedAmounts.Count)
            {
                changedAmounts.Clear();
                foreach (SkillChange skillChange in Props.skillChanges)
                {
                    if (skillChange.skill == null || Pawn.skills.GetSkill(skillChange.skill) != null)
                        changedAmounts.Add(skillChange.skillChange);
                }
            }

            foreach (SkillChange skillChange in Props.skillChanges)
            {
                SkillRecord skill;
                if (skillChange.skill == null)
                {
                    if (changedSkills.NullOrEmpty()) continue;
                    skill = Pawn.skills.GetSkill(changedSkills[noSkillCounter]);
                    noSkillCounter++;
                }
                else skill = Pawn.skills.GetSkill(skillChange.skill);

                skill.Level -= changedAmounts[originalCounter];
                if (!originalPassions.NullOrEmpty()) skill.passion = originalPassions[originalCounter];
                originalCounter++;
            }
        }

        private bool Redundant(SkillRecord skill, SkillChange skillChange)
        {
            if (skillChange.skillChange != 0) return false;
            if (skillChange.setPassion)
            {
                if (skill.passion == skillChange.passion) return true;
            }
            else if (skillChange.passionChange < 0 && skill.passion == Passion.None) return true;
            else if (skillChange.passionChange > 0 && skill.passion == skill.passion.IncrementPassion()) return true;
            return false;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref changedSkills, "EBSG_hediffChangedSkills");
            Scribe_Collections.Look(ref changedAmounts, "EBSG_hediffChangedAmounts");
            Scribe_Collections.Look(ref originalPassions, "EBSG_hediffOriginalPassions");
        }
    }
}

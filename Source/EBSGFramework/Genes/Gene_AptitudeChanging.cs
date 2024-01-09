using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace EBSGFramework
{
    public class Gene_AptitudeChanging : HediffAdder
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (!def.HasModExtension<EBSGExtension>()) Log.Error(def + " is missing EBSGExtension");
            else
            {
                EBSGExtension extension = def.GetModExtension<EBSGExtension>();
                if (extension.skillChanges.NullOrEmpty()) Log.Error(def + " has EBSGExtension, but has no changes set");
                else
                {
                    List<SkillDef> changedSkills = new List<SkillDef>();
                    foreach (SkillChange skillChange in extension.skillChanges)
                    {
                        SkillRecord skill;
                        if (skillChange.skill == null)
                        {
                            IEnumerable<SkillRecord> validSkills = pawn.skills.skills.Where((SkillRecord s) => !s.TotallyDisabled && !changedSkills.Contains(s.def) && !Redundant(s, skillChange));
                            if (validSkills.EnumerableNullOrEmpty()) continue;
                            skill = validSkills.RandomElement();
                        }
                        else skill = pawn.skills.GetSkill(skillChange.skill);
                        if (skill == null) continue;

                        skill.Level += skillChange.skillIncrease;

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
            }
        }

        private bool Redundant(SkillRecord skill, SkillChange skillChange)
        {
            if (skillChange.skillIncrease != 0) return false;
            if (skillChange.setPassion)
            {
                if (skill.passion == skillChange.passion) return true;
            }
            else if (skillChange.passionChange < 0 && skill.passion == Passion.None) return true;
            else if (skillChange.passionChange > 0 && skill.passion == skill.passion.IncrementPassion()) return true;
            return false;
        }
    }
}

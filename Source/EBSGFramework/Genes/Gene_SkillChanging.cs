using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace EBSGFramework
{
    public class Gene_SkillChanging : HediffAdder
    {
        public List<SkillDef> changedSkills;
        public List<int> changedAmounts;
        public List<Passion> originalPassions;
        public int delayTicks;

        public override void PostAdd()
        {
            base.PostAdd();
            if (!def.HasModExtension<EBSGExtension>()) Log.Error(def + " is missing EBSGExtension");
            else
            {
                EBSGExtension extension = def.GetModExtension<EBSGExtension>();
                if (extension.skillChanges.NullOrEmpty()) Log.Error(def + " has EBSGExtension, but has no changes set");
                else delayTicks = 70; // Wait until post spawn plus a little over a second before changing passions to allow for things like the gene randomizer to take effect
            }
        }

        public override void Tick()
        {
            if (!pawn.Spawned) return;
            if (delayTicks >= 0) delayTicks--;
            if (delayTicks == 0)
            {
                if (changedSkills == null) changedSkills = new List<SkillDef>();
                if (changedAmounts == null) changedAmounts = new List<int>();
                if (originalPassions == null) originalPassions = new List<Passion>();

                EBSGExtension extension = def.GetModExtension<EBSGExtension>();
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
        }

        public override void PostRemove()
        {
            base.PostRemove();


            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension != null)
            {
                int noSkillCounter = 0;
                int originalCounter = 0;
                if (changedSkills == null) changedSkills = new List<SkillDef>();
                if (changedAmounts == null) changedAmounts = new List<int>();
                if (originalPassions == null) originalPassions = new List<Passion>();

                if (changedSkills.Count != changedAmounts.Count)
                {
                    changedAmounts.Clear();
                    foreach (SkillChange skillChange in extension.skillChanges)
                    {
                        changedAmounts.Add(skillChange.skillChange);
                    }
                }

                foreach (SkillChange skillChange in extension.skillChanges)
                {
                    SkillRecord skill;
                    if (skillChange.skill == null)
                    {
                        if (changedSkills.NullOrEmpty()) continue;
                        skill = pawn.skills.GetSkill(changedSkills[noSkillCounter]);
                        noSkillCounter++;
                    }
                    else skill = pawn.skills.GetSkill(skillChange.skill);

                    skill.Level -= changedAmounts[originalCounter];
                    if (!originalPassions.NullOrEmpty()) skill.passion = originalPassions[originalCounter];
                    originalCounter++;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref changedSkills, "changedSkills");
            Scribe_Collections.Look(ref changedAmounts, "changedAmounts");
            Scribe_Collections.Look(ref originalPassions, "originalPassions");
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
    }
}

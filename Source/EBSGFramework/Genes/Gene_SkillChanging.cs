using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class Gene_SkillChanging : HediffAdder
    {
        public List<SkillDef> changedSkills;
        public List<int> changedAmounts;
        public List<Passion> originalPassions;

        public override void PostAdd()
        {
            if (!Active || Overridden) return;
            base.PostAdd();
            if (!def.HasModExtension<EBSGExtension>()) Log.Error(def + " is missing EBSGExtension");
            else
            {
                if (Extension.skillChanges.NullOrEmpty()) Log.Error(def + " has EBSGExtension, but has no changes set");
                else
                {
                    if (changedSkills == null) changedSkills = new List<SkillDef>();
                    if (changedAmounts == null) changedAmounts = new List<int>();
                    if (originalPassions == null) originalPassions = new List<Passion>();

                    foreach (SkillChange skillChange in Extension.skillChanges)
                    {
                        SkillRecord skill;
                        if (skillChange.skill == null)
                        {
                            IEnumerable<SkillRecord> validSkills = pawn.skills.skills.Where(s => !s.TotallyDisabled && !changedSkills.Contains(s.def) && !Redundant(s, skillChange));
                            if (validSkills.EnumerableNullOrEmpty()) continue;
                            skill = validSkills.RandomElement();
                        }
                        else skill = pawn.skills.GetSkill(skillChange.skill);
                        if (skill == null) continue;

                        changedAmounts.Add(skillChange.skillChange.RandomInRange);
                        originalPassions.Add(skill.passion);
                        changedSkills.Add(skill.def);

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
                    }
                    pawn.skills.DirtyAptitudes();
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();

            if (Extension != null)
            {
                if (!changedSkills.NullOrEmpty())
                {
                    int counter = 0;
                    foreach (SkillDef skill in changedSkills)
                    {
                        SkillRecord record = pawn.skills.GetSkill(skill);
                        if (record != null)
                            record.passion = originalPassions[counter];
                        counter++;
                    }
                }
                pawn.skills.DirtyAptitudes();
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
            if (skillChange.skillChange != new IntRange(0, 0)) return false;
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

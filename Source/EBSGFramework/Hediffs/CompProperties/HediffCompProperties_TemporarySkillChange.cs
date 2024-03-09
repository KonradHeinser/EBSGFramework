using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_TemporarySkillChange : HediffCompProperties
    {
        public List<SkillChange> skillChanges;

        public HediffCompProperties_TemporarySkillChange()
        {
            compClass = typeof(HediffComp_TemporarySkillChange);
        }
    }
}

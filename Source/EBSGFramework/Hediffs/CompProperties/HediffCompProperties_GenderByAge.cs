using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_GenderByAge : HediffCompProperties
    {
        public List<GenderByAge> genderByAge;

        public bool revertPostRemove = false;

        public bool saveBeard = false;

        public HediffCompProperties_GenderByAge()
        {
            compClass = typeof(HediffComp_GenderByAge);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;
            if (genderByAge.NullOrEmpty())
                yield return "genderByAge doesn't have any items in it";
        }
    }
}

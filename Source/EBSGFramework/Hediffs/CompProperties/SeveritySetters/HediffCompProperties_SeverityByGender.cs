using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByGender : HediffCompProperties
    {
        public List<GenderByAge> genders = new List<GenderByAge>();

        public float defaultSeverity;
        
        public HediffCompProperties_SeverityByGender()
        {
            compClass = typeof(HediffComp_SeverityByGender);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;
            if (genders.NullOrEmpty())
                yield return "genders is not set";
        }
    }
}
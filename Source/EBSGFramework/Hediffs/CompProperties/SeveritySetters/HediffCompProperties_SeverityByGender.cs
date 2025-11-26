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
    }
}
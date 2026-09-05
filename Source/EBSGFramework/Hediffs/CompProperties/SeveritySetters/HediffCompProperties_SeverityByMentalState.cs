using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByMentalState : HediffCompProperties
    {
        public float defaultSeverity = 1;

        public float draftedSeverity = 2;

        public float fightingSeverity = 2; // This handles auto attacking without being drafted
        
        public bool addSeverityPerHour;

        public List<MentalStateEffect> mentalStateEffects;

        public HediffCompProperties_SeverityByMentalState()
        {
            compClass = typeof(HediffComp_SeverityByMentalState);
        }
    }
}

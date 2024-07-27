using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByMentalState : HediffCompProperties
    {
        public float defaultSeverity = 1;

        public float draftedSeverity = 2;

        public float fightingSeverity = 2; // This handles autoattacking without being drafted

        public List<MentalStateEffect> mentalStateEffects;

        public HediffCompProperties_SeverityByMentalState()
        {
            compClass = typeof(HediffComp_SeverityByMentalState);
        }
    }
}

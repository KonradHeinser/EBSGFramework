using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompProperties_AbilityStopMentalNoPsychic : CompProperties_AbilityEffect
    {
        public List<MentalStateDef> exceptions;

        public bool mustHaveMentalState = true;

        public float minorChance = 1f;

        public float majorChance = 0.5f;

        public float extremeChance = 0.25f;

        public CompProperties_AbilityStopMentalNoPsychic()
        {
            compClass = typeof(CompAbilityEffect_StopMentalNoPsychic);
        }
    }
}

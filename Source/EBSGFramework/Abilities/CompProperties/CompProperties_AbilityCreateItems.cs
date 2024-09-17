using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityCreateItems : CompProperties_AbilityEffect
    {
        public List<ThingPatternPart> thingPattern;

        public HediffDef linkingHediff;

        public bool sendSkipSignal = true;

        public bool pollutedForbidden = false;

        public bool pollutedRequired = false;

        public bool waterForbidden = false;

        public bool waterRequired = false;

        public bool roofForbidden = false;

        public bool roofRequired = false;

        public bool noPushing = false;

        public bool noPlants = false;

        public bool noBuildings = true;

        public CompProperties_AbilityCreateItems()
        {
            compClass = typeof(CompAbilityEffect_CreateItems);
        }
    }
}

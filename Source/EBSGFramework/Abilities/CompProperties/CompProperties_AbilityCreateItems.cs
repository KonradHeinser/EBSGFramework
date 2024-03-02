using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityCreateItems : CompProperties_AbilityEffect
    {
        public List<ThingPatternPart> thingPattern;

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

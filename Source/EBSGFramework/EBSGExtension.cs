using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGExtension : DefModExtension
    {
        public SimpleCurve peopleToMoodCurve;
        public GeneDef relatedGene;
        public bool checkNotPresent = false;

        public List<HediffsToParts> hediffsToApply;
        public bool vanishingGene = false;

        // Curves that can be added to a gene to give pawns an additional age multiplier
        public SimpleCurve fertilityAgeAdditionalFactor;
        public SimpleCurve maleFertilityAgeAdditionalFactor;
        public SimpleCurve femaleFertilityAgeAdditionalFactor;

        // Sets minimum fertility based on age
        public SimpleCurve minFertilityByAgeFactor;
        public SimpleCurve minMaleFertilityByAgeFactor;
        public SimpleCurve minFemaleFertilityByAgeFactor;

        // Sets maximum fertility based on age
        public SimpleCurve maxFertilityByAgeFactor;
        public SimpleCurve maxMaleFertilityByAgeFactor;
        public SimpleCurve maxFemaleFertilityByAgeFactor;

        // Static min fertility
        public float minFertility = 0;
        public float minMaleFertility = 0;
        public float minFemaleFertility = 0;

        // Static max fertility
        public float maxFertility = 999999;
        public float maxMaleFertility = 999999;
        public float maxFemaleFertility = 999999;
    }
}

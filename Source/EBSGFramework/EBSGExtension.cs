using Verse;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class EBSGExtension : DefModExtension
    {
        public SimpleCurve peopleToMoodCurve;
        public GeneDef relatedGene;
        public List<GeneDef> relatedGenes;
        public bool checkNotPresent = false;
        public List<int> thresholds;
        public List<GeneticMultiplier> geneticMultipliers;

        public List<HediffsToParts> hediffsToApply;
        public bool vanishingGene = false;
        public FloatRange expectedAges;
        public FloatRange ageRange;
        public bool sameBioAndChrono = false;
        public bool chronicAgeRemoval = true;

        public List<SkillChange> skillChanges;

        // Aquatic Gene Stuff
        public List<BiomeDef> goodBiomes;
        public List<BiomeDef> badBiomes;
        public float maxWaterDistance = 0; // If above 0, this extends the search radius
        public bool waterSatisfiedByRain = true; // Causes the carrier to gain the in/out of water effects while standing in the rain
        public float minimumRainAmount = 0; // Checks for any number above, but not including this. Only use if you're really sure of this
        public int waterTilesNeeded = 1; // Normally just stops after the first tile
        public bool biomeOverridesWater = false; // While true, the code doesn't check for water unless in a neutral biome (unlisted)

        public List<HediffDef> hediffsWhileInWater;
        public List<HediffDef> hediffsWhileOutOfWater;
        public List<HediffDef> hediffsWhileInGoodBiome; // Stacks with in and out of water hediffs
        public List<HediffDef> hediffsWhileInBadBiome;
        public List<HediffDef> hediffsWhileRaining;

        public List<NeedOffset> needOffsetsPerHourInWater;
        public List<NeedOffset> needOffsetsPerHourNotInWater;
        public List<NeedOffset> needOffsetsPerHourInGoodBiome;
        public List<NeedOffset> needOffsetsPerHourInBadBiome;
        public List<NeedOffset> needOffsetsPerHourWhileRaining;

        // Used in ThoughtWorker_Gene_GeneSocial
        public bool compoundingHatred = false; // When true, each gene that is found in checked genes increases the stage
        public bool opinionOfAllOthers = false; // Makes the thought apply to all others instead of just those with checked genes. Intended for xenophobe/xenophile
        public List<GeneDef> checkedGenes; // Genes checked for opinions
        public List<GeneDef> nullifyingGenes; // Genes checked for early nullification. These cause the thought to never appear
        public List<GeneDef> requiredGenes; // The observer musthave one of these genes to feel anything. Acts as a reverse nullifyingGenes

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

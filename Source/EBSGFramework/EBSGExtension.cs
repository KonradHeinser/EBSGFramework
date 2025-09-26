using Verse;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class EBSGExtension : DefModExtension
    {
        public List<GeneDef> relatedGenes;
        public bool checkEvolutionsPostAdd = false;
        public List<GeneticEvolution> geneticEvolutions;
        public int maxEvolutions = 1;
        public bool keepEvolvingGene = false;
        public bool recoverEvolutions = true;

        public bool hideInGeneTabWhenInactive = false;
        public bool hideAllInactiveGenesForXenotype = false;
        public bool hideAllInactiveSkinColorGenesForXenotype = false;
        public bool hideAllInactiveHairColorGenesForXenotype = false;
        public List<GeneticMultiplier> geneticMultipliers;
        public float universalMoodFactor = 1;
        public List<GenderByAge> genderByAge;
        public List<HediffToParts> hediffsToGivePostConsumption;

        public EffecterDef effecter;

        public HediffDef alternativePregnancyHediff;
        public List<HediffToParts> hediffsToApply;
        public bool vanishingGene = false;
        public FloatRange expectedAges;
        public FloatRange? ageRange = null;
        public bool sameBioAndChrono = false;
        public bool chronicAgeRemoval = true;

        public float maxAgeActive = -1f;
        public float removePastAge = -1f;
        public List<HediffToParts> hediffsToApplyAtAges;

        public List<AbilityAndGeneLink> geneAbilities;
        public List<GeneDef> conflictingGenes; // List of very specific genes to make it incompatible with

        public float newBaseValue = -1f;
        public int clotCheckInterval = 360; // Shouldn't be below 60, but I won't force it. This just determines how often it tries to heal, so a lower number means it's less likely that there will be a delay in clotting
        public FloatRange tendQuality = new FloatRange(0.2f, 0.7f);

        public TargetingParameters targetParams = null;
        // Raceprop conditionals
        public bool allowHumanlikes = true;
        public bool allowDryads = true;
        public bool allowInsects = true;
        public bool allowAnimals = true;
        public bool allowMechanoids = true;
        public bool allowEntities = true;

        // For mutating genes
        public List<RandomXenoGenes> mutationGeneSets; // Named as such because it was originally solely to create entire xenotypes
        public bool removeGenesFromOtherLists = true; // This being true means that while activating, the comp will remove any gene that exists on the other list(s), even if they are from the xenotype
        public bool inheritable = true; // The default behaviour is to make the genes inheritable(germline)
        public int delayTicks = 10; // How long it waits until triggering. Should wait at least a few ticks
        public List<SkillChange> skillChanges;

        // Aquatic Gene Stuff
        public List<BiomeDef> amazingBiomes;
        public List<BiomeDef> greatBiomes;
        public List<BiomeDef> goodBiomes;
        public List<BiomeDef> badBiomes;
        public List<BiomeDef> terribleBiomes;
        public List<BiomeDef> abysmalBiomes;
        public float maxWaterDistance = 0; // If above 0, this extends the search radius
        public bool waterSatisfiedByRain = true; // Causes the carrier to gain the in/out of water effects while standing in the rain
        public float minimumRainAmount = 0; // Checks for any number above, but not including this. Only use if you're really sure of this
        public int waterTilesNeeded = 1; // Normally just stops after the first tile
        public bool biomeOverridesWater = false; // While true, the code doesn't check for water unless in a neutral biome (unlisted)

        public List<HediffDef> hediffsWhileInWater;
        public List<HediffDef> hediffsWhileOutOfWater;

        // Stacks with in and out of water hediffs
        public List<HediffDef> hediffsWhileInAmazingBiome;
        public List<HediffDef> hediffsWhileInGreatBiome;
        public List<HediffDef> hediffsWhileInGoodBiome;
        public List<HediffDef> hediffsWhileInBadBiome;
        public List<HediffDef> hediffsWhileInTerribleBiome;
        public List<HediffDef> hediffsWhileInAbysmalBiome;
        public List<HediffDef> hediffsWhileRaining;

        public List<NeedOffset> needOffsetsPerHourInWater;
        public List<NeedOffset> needOffsetsPerHourNotInWater;

        // Stacks with in and out of water need offsets
        public List<NeedOffset> needOffsetsPerHourInAmazingBiome;
        public List<NeedOffset> needOffsetsPerHourInGreatBiome;
        public List<NeedOffset> needOffsetsPerHourInGoodBiome;
        public List<NeedOffset> needOffsetsPerHourInBadBiome;
        public List<NeedOffset> needOffsetsPerHourInTerribleBiome;
        public List<NeedOffset> needOffsetsPerHourInAbysmalBiome;
        public List<NeedOffset> needOffsetsPerHourWhileRaining;

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

        // For Needs
        public bool displayLowAlert = false;
        public List<float> thresholdPercentages = new List<float> { 0.3f };
        
        public float minAgeForNeed = 13f; // Only used for need classes like murderous, which wouldn't work very well on children
        public float maxAgeForNeed = 9999f;
        public float increasePerKill = 1f;
        public float increasePerMeleeKill = 0f;
        public float increasePerRangedKill = 0f;

        public HediffDef hediffWhenEmpty;
        public float initialSeverity = 0.001f;
        public float risePerDayWhenEmpty = 0.2f;
        public float fallPerDayWhenNotEmpty = 0.1f;
        public StatDef fallStat;

        public NeedDef need;
        public SimpleCurve moodOffsetCurve;
        public StatDef riseStat;

        // Building
        public List<NeedOffset> needOffsetsPerHour;
        public SoundDef startSound;
        public SoundDef sustainerSound;
        public SoundDef endSound;
        public ThingDef chargeMote;
        public ThingDef chargeMotePulse;
        public EffecterDef wasteProducedEffecter;
        public float wastePerHourOfUse; // Only applies if there's a WasteProducer comp
        public List<GeneEffect> resourceOffsetsPerHour; // For DRG in the pawn need charger
        public bool negativeNeedOffsetsAreNotCosts = false;
        public bool negativeResourceOffsetsAreNotCosts = false;

        public ThingDef relatedThing;
        public JobDef relatedJob;
        public float maxStorage = 10f;
        public float staticUsePerDay = -1f;

        public HediffToParts endHediff;
        public HediffToParts mechEndHediff;
        public ThingDef filth;

        public PawnKindDef pawnKind;
        public IntRange countRange = IntRange.One;
        public SoundDef sound;
        public string message;
        public FloatRange bioAge = FloatRange.Zero;
        public FloatRange chronoAge = FloatRange.Zero;
        public MentalStateDef mentalState;

        // Recipe Stuff
        public List<List<ThingDefCountClass>> thingCountList;
        public bool staticQuality = false; // Only need set to true if there are products that have quality. Will set for all of them at once.

        // Downed Thoughts
        public ThoughtDef downedMemory;
        public ThoughtDef downedByPawnMemory;
        public ThoughtDef downedByHumanlikeMemory;
        public ThoughtDef downedByMechMemory;
        public ThoughtDef downedByAnimalMemory;
        public ThoughtDef downedByInsectMemory;
        public ThoughtDef downedByEntityMemory;
        public List<HediffDef> ignoredHediffsCausingDowning;
    }
}

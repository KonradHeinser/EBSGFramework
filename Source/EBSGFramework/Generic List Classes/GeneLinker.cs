using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class GeneLinker
    {
        public GeneDef mainResourceGene;

        public float amount = 0f;

        public bool usesGainStat;

        public bool usePassiveStat;

        public int ticks = 100;

        public EffecterDef consumeEffect; // Similar to ingestEffect

        public SoundDef consumeSound; // Similar to ingestSound

        public string consumptionReportString; // Similar to ingestReportString

        public string floatMenuString; // Uses similar syntax as consumptionReportString, but is what is displayed when someone right clicks the consumable

        public HoldOffsetSet consumeHoldOffset; // Similar to ingestHoldOffsetStanding

        public FloatRange validSeverity = FloatRange.Zero;

        public FloatRange validResourceLevels = FloatRange.ZeroToOne;

        public bool removeWhenLimitsPassed = false;

        public float maxDistance = 4.9f;

        public bool allowHumanoids = true;

        public bool allowDryads = true;

        public bool allowMechanoids = true;

        public bool allowInsects = true;

        public bool allowAnimals = true;

        public bool allowEntities = true;

        public List<GeneDef> forbiddenTargetGenes;
    }
}

using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class GeneLinker
    {
        public GeneDef mainResourceGene;

        public float amount = 0f;

        public bool usesGainStat = true;

        public int ticks = 100;

        public EffecterDef consumeEffect; // Similar to ingestEffect

        public SoundDef consumeSound; // Similar to ingestSound

        public string consumptionReportString; // Similar to ingestReportString

        public string floatMenuString; // Uses similar syntax as consumptionReportString, but is what is displayed when someone right clicks the consumable

        public HoldOffsetSet consumeHoldOffset; // Similar to ingestHoldOffsetStanding

        public float minSeverity = 0f;

        public float maxSeverity = 99999f;

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

using UnityEngine;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class DRGExtension : DefModExtension
    {
        // Tied to ResourceGene
// This needs to be set to true on the main gene so some of the things can find the main gene. If vanilla was set up this way, Hemogenic would be True, while HemogenicDrain would be false
        public bool isMainGene = false; 
// resourcePacks is for the main resource packs/ingestibles, and should not include generic ingestibles(i.e. meat, eggs, etc.) unless A) those are the primary source, and B) you plan on adding all, or at least a large number, of them
        public List<ThingDef> resourcePacks;
        public bool resourcePacksAllowed = true; // If the resourcePacks is empty because you have no main resource sources, set this to false
        public HediffDef cravingHediff; // Optional
        public HediffDef overchargeHediff; // Optional
        public StatDef gainStat; // Optional
        public float maximum = 1f; // This is the default for Hemogen. Optional
        public StatDef maxStat; // Overrides maximum. Optional
        public StatDef maxFactorStat; // Works with both maximum options. Optional
        public Color barColor = new ColorInt(138, 3, 3).ToColor; // This is the bar color when not hovering over it
        public Color barHighlightColor = new ColorInt(145, 42, 42).ToColor; // This is the bar color when hovering over it
        public ThingDef iconThing; // This changes the little icon on the label to something other than a hemogen pack

        // Ingestion stuff - Offsets resource based on type. Also tied to ResourceGene
        public bool checkIngestion = false; // If this is false, then all of the below effects are ignored
        public float eggIngestionEffect = 0f; // Fertilized and unfertilized. As far as I know, it only counts raw eggs
        public float drugIngestionEffect = 0f; // Probably won't do a whole lot since most drugs don't have nutrition
        public float corpseIngestionEffect = 0f; // Still not completely sure if this works
        public float humanlikeIngestionEffect = 0f; // For hemogen this is 0.0375
        public float genericMeatIngestionEffect = 0f; // This is just raw meat

        // Tied to ResourceDrainGene
        public GeneDef mainResourceGene;

    }
}

using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_GiveHediffsInRangeGeneRestricting : HediffCompProperties
    {
        public float range;

        public List<GeneDef> forbiddenGenes;

        public TargetingParameters targetingParameters;

        public HediffDef hediff;

        public ThingDef mote;

        public bool hideMoteWhenNotDrafted;

        public float initialSeverity = 1f;

        public bool onlyPawnsInSameFaction = true;

        public HediffCompProperties_GiveHediffsInRangeGeneRestricting()
        {
            compClass = typeof(HediffComp_GiveHediffsInRangeGeneRestricting);
        }
    }
}

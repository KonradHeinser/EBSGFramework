using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class GeneLinker
    {
        public GeneDef mainResourceGene;

        public float amount = 0f;

        public StatDef statFactor;

        public int ticks = 100;

        public EffecterDef consumeEffect; // Equal to ingestEffect

        public SoundDef consumeSound; // Equal to ingestSound

        public string consumptionReportString; // Equal to ingestReportString

        public HoldOffsetSet consumeHoldOffset; // Equal to ingestHoldOffsetStanding
    }
}

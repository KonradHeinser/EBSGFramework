using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class GeneLinker
    {
        public GeneDef mainResourceGene;

        public float amount = 0f;

        public int ticks = 100;

        public EffecterDef consumeEffect; // Similar to ingestEffect

        public SoundDef consumeSound; // Similar to ingestSound

        public string consumptionReportString; // Similar to ingestReportString

        public string floatMenuString; // Uses similar syntax as consumptionReportString, but is what is displayed when someone right clicks the consumable

        public HoldOffsetSet consumeHoldOffset; // Similar to ingestHoldOffsetStanding
    }
}

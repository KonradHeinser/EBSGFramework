using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ComaExtension : DefModExtension
    {
        // Required
        public HediffDef comaRestingHediff;
        public HediffDef comaInterruptedHediff;
        public HediffDef exhaustionHediff;
        public JobDef relatedJob;
        public string noun;
        public string gerund;

        // Recommended
        public float gainPerDayComatose = 0.2f;
        public int minComaTicks = 300000; // 5 days
        public bool usesBuildings = true;
        public int baseBuildingMax = 1;
        public string wakeCommandTexPath;
        public string autoWakeTexCommandPath;

        // Optional
        public bool needBedOutOfSunlight = false;
        public ThingDef restingMote;
        public string chamberName;
        public StatDef fallStat;
        public StatDef riseStat;

        // For linking to the gene
        public GeneDef relatedGene;
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ComaExtension : DefModExtension
    {
        public int baseBuildingMax = 1;
        public string nextComaTranslateString;
        public string shouldComaTranslateString;
        public bool needBedOutOfSunlight = false;
        public NeedDef relatedNeed;
        public HediffDef comaRestingHediff;
        public HediffDef comaInterruptedHediff;
        public HediffDef exhaustionHediff;
        public float gainPerDayComatose = 0.2f;
        public ThingDef restingMote;
        public string chamberName;
        public string startRestLabel;
        public JobDef relatedJob;
        public StatDef fallStat;
        public StatDef riseStat;
        public GeneDef relatedGene;
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_GiveHediffsToNonAlliesInRange : HediffCompProperties
    {
        public float range = 1;
        public StatDef rangeStat = null; // Overrides range
        public TargetingParameters targetingParameters;
        public HediffDef hediff;
        public ThingDef mote;
        public bool hideMoteWhenNotDrafted;
        public bool psychic = true;
        public float initialSeverity = 1f;
        public bool onlyWhileDrafted = true;

        public HediffCompProperties_GiveHediffsToNonAlliesInRange()
        {
            compClass = typeof(HediffComp_GiveHediffsToNonAlliesInRange);
        }
    }
}

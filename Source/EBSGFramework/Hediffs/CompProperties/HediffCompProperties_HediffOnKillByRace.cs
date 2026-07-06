using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HediffOnKillByRace : HediffCompProperties
    {
        public float severity = 1;
        
        public float addedSeverity = 1;
        
        public bool multiplySeverityByBodySize;
        
        public bool multiplyAddedSeverityByBodySize;

        public HediffDef defaultHediff; // Any race not covered by any other section

        public HediffDef humanlike;
        
        public HediffDef animal;

        public HediffDef mechanoid;

        public HediffDef insect;

        public HediffDef entity;
        
        public HediffDef dryad;

        public HediffCompProperties_HediffOnKillByRace()
        {
            compClass = typeof(HediffComp_HediffOnKillByRace);
        }
    }
}
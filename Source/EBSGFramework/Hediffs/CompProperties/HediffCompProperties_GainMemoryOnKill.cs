using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_GainMemoryOnKill : HediffCompProperties
    {
        public ThoughtDef memory; // This is only used if there's no other option available. If this is left blank, memories are only generated when certain things are killed

        public ThoughtDef animalMemory;
        
        public ThoughtDef humanMemory;
        
        public ThoughtDef mechanoidMemory;
        
        public ThoughtDef insectMemory;
        
        public ThoughtDef dryadMemory;
        
        public ThoughtDef entityMemory;
        
        public HediffCompProperties_GainMemoryOnKill()
        {
            compClass = typeof(HediffComp_GainMemoryOnKill);
        }
    }
}
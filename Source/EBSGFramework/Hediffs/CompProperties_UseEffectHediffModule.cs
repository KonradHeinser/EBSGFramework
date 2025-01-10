using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_UseEffectHediffModule : CompProperties_UseEffect
    {
        // List of slot IDs that this module can be installed into
        public List<string> slotIDs;
        // Amount of slot capacity that this module takes
        public float requiredCapacity = 0;
        // List of IDs that this module is incompatible with
        public List<string> excludeIDs;
        // If this module can be ejected
        public bool ejectable = true;
        // Sound that is played when the module is installed
        public SoundDef installSound;
        // Sound that is played when the module is ejected
        public SoundDef ejectSound;
        // List of stage modifiers. Has to either be null, empty, a single element or equal to the parent hediff's amount of stages
        public List<StageOverlay> stageOverlays;
        // List of comps that are applied when this module is attached
        public List<HediffCompProperties> comps;
        // List of hediffs that are applied to the parent part when this module is attached
        public List<HediffDef> hediffs;
        // Prerequisite properties for the module, with the module only be able to be installed if the pawn fits the criteria. Assign the values in the props as you would normally
        public Prerequisites prerequisites;

        public CompProperties_UseEffectHediffModule()
        {
            compClass = typeof(CompUseEffect_HediffModule);
        }
    }
}

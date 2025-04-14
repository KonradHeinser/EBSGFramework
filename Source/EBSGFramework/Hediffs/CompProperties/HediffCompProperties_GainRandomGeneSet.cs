using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_GainRandomGeneSet : HediffCompProperties
    {
        public List<RandomXenoGenes> geneSets; // Named as such because it was originally solely to create entire xenotypes
        public bool removeGenesFromOtherLists = true; // This being true means that while activating, the comp will remove any gene that exists on the other list(s), even if they are from the xenotype
        public bool inheritable = true; // The default behaviour is to make the genes inheritable(germline)
        public int delayTicks = 10; // How long it waits until triggering. Should wait at least a few ticks
        public List<GeneDef> alwaysAddedGenes; // These genes will be added regardless of the set picked
        public List<GeneDef> alwaysRemovedGenes; // These genes will be removed regardless of set picked

        public float minSeverity = 0; // Sets the minimum severity that the hediff must be at to start gene generation. Once this is reached, the hediff will disappear shortly after
        public float maxSeverity = 999; // If a hediff goes down in severity, use this instead of min
        public FloatRange validSeverity = FloatRange.Zero;
        public bool removeHediffAfterwards = true; // Only set to false if the hediff has other comps that you want to keep around.
        public bool showMessage = true; // Give message when done

        public HediffCompProperties_GainRandomGeneSet()
        {
            compClass = typeof(HediffComp_GainRandomGeneSet);
        }
    }
}

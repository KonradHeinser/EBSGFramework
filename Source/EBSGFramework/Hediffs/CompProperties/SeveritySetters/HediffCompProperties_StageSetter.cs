using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_StageSetter : HediffCompProperties
    {
        public string label;

        public string description;

        public string iconPath;

        public bool usableDuringCooldowns = false;

        public List<StageSet> sets;

        public HediffCompProperties_StageSetter()
        {
            compClass = typeof(HediffComp_StageSetter);
        }
    }

    public class StageSet
    {
        public string label;

        public Prerequisites prerequisites;

        public string iconPath;

        public int ticks = 0;
    }
}

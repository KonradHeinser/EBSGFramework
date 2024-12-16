using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompProperties_SleepCasketPower : CompProperties_Power
    {
        public List<StatDef> statFactors;

        public List<StatDef> statDivisors;

        public CompProperties_SleepCasketPower()
        {
            compClass = typeof(CompPowerPlantSleepCasket);
        }

    }
}

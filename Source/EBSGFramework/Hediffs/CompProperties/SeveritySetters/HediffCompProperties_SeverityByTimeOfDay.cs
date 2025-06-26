using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByTimeOfDay : HediffCompProperties
    {
        public SimpleCurve timeToSeverityCurve;

        public HediffCompProperties_SeverityByTimeOfDay()
        {
            compClass = typeof(HediffComp_SeverityByTimeOfDay);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (timeToSeverityCurve == null)
                yield return "timeToSeverityCurve needs to have something in it so severity can be calculated.";
        }
    }
}

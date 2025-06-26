using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByLightLevel : HediffCompProperties
    {
        public SimpleCurve lightToSeverityCurve;

        public SimpleCurve timeToSeverityCurve; // Back up plan

        public HediffCompProperties_SeverityByLightLevel()
        {
            compClass = typeof(HediffComp_SeverityByLightLevel);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (lightToSeverityCurve == null)
                yield return "A lightToSeverityCurve is required to calculate a severity.";
        }
    }
}

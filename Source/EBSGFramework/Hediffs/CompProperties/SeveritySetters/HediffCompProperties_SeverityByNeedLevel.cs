using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByNeedLevel : HediffCompProperties
    {
        public NeedDef need;

        public bool usePercentage = false;
        
        public SimpleCurve severity;

        public HediffCompProperties_SeverityByNeedLevel()
        {
            compClass = typeof(HediffComp_SeverityByNeedLevel);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string err in base.ConfigErrors(parentDef))
                yield return err;
            if (need == null)
                yield return "need is not set";
            if (severity == null || severity.PointsCount <= 0)
                yield return "severity is not set";
        }
    }
}
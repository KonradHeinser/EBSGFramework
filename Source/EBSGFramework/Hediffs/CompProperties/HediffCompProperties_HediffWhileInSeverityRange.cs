using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HediffWhileInSeverityRange : HediffCompProperties
    {
        public List<HediffSeverityLevel> hediffsAtSeverities;

        public HediffCompProperties_HediffWhileInSeverityRange()
        {
            compClass = typeof(HediffComp_HediffWhileInSeverityRange);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;
            if (hediffsAtSeverities.NullOrEmpty())
                yield return "HediffsAtSeverities must have at least one item";
        }
    }
}

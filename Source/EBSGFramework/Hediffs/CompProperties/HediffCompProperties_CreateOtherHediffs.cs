using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_CreateOtherHediffs : HediffCompProperties
    {
        public List<HediffsAtSeverities> hediffSets; // This allows for customizing both when the hediffs are applied, and how much they are applied for
        public float minAge = 0;

        public HediffCompProperties_CreateOtherHediffs()
        {
            compClass = typeof(HediffComp_CreateOtherHediffs);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (hediffSets.NullOrEmpty())
                yield return "hediffSets needs to be set.";
        }
    }
}

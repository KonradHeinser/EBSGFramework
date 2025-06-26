using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TemporaryGenes : HediffCompProperties
    {
        public List<GenesAtSeverity> genesAtSeverities;

        public HediffCompProperties_TemporaryGenes()
        {
            compClass = typeof(HediffComp_TemporaryGenes);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (genesAtSeverities.NullOrEmpty())
                yield return "genesAtSeverities needs to be set.";
        }
    }
}

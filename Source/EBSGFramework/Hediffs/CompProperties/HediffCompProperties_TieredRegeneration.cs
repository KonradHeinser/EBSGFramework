using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TieredRegeneration : HediffCompProperties
    {
        public List<RegenSet> regenSets;
        public bool healWhileRegrowing = false; // By default, regrowing parts stops normal regen completely
        public bool prioritizeHeal = false; // By default, missing part restoration is prioritized over generic healing
        public HediffCompProperties_TieredRegeneration()
        {
            compClass = typeof(HediffComp_TieredRegeneration);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in ConfigErrors(parentDef))
                yield return error;

            if (regenSets.NullOrEmpty())
                yield return "regenSets needs to be set.";
        }
    }
}

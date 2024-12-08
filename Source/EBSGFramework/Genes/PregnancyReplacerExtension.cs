using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class PregnancyReplacerExtension : DefModExtension
    {
        public List<GeneDef> fatherRequiresOneOf; // If the father doesn't have one of these genes, then the pregnancy doesn't get replaced or create a thing, it just goes away

        public List<ThingDefCountClass> spawnThings;

        public HediffDef motherHediff;

        public HediffDef fatherHediff;

        public List<HediffDef> replacementHediffs;

        public float initialSeverity = 1f;

        public float increaseSeverity = 0f;

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);
    }
}

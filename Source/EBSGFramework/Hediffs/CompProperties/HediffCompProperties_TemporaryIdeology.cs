using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TemporaryIdeology : HediffCompProperties
    {
        public FactionDef factionOfIdeo;

        public FloatRange certainty = FloatRange.One;

        public bool temporary = true;

        public HediffCompProperties_TemporaryIdeology()
        {
            compClass = typeof(HediffComp_TemporaryIdeology);
        }
    }
}

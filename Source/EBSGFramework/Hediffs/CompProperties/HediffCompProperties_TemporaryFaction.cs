using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TemporaryFaction : HediffCompProperties
    {
        public FactionDef staticFaction;

        public bool useStatic = false;

        public bool temporary = true;

        public HediffCompProperties_TemporaryFaction()
        {
            compClass = typeof(HediffComp_TemporaryFaction);
        }
    }
}

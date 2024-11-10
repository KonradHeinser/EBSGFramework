using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_AlterXenotype : HediffCompProperties
    {
        public XenotypeDef xenotype;

        public FloatRange severities = new FloatRange(0f, 999f);

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public bool setXenotype = true; // Clears old xeno genes and replaces them with the xenotype's

        public bool sendMessage = true;

        public HediffCompProperties_AlterXenotype()
        {
            compClass = typeof(HediffComp_AlterXenotype);
        }
    }
}

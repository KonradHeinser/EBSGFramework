using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_UseEffectOffsetComaCapacity : CompProperties_UseEffect
    {
        public int offset;

        public GeneDef gene;

        public CompProperties_UseEffectOffsetComaCapacity()
        {
            compClass = typeof(CompUseEffect_OffsetComaCapacity);
        }
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_Launch : CompProperties_AbilityEffect
    {
        public int maxDistance = 10;

        public ThingDef skyfallerLeaving;

        public WorldObjectDef worldObject;

        public CompProperties_Launch()
        {
            compClass = typeof(CompAbilityEffect_Launch);
        }
    }
}

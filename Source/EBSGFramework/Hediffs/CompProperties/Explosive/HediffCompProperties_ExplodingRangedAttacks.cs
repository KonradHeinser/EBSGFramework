using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_ExplodingRangedAttacks : HediffCompProperties
    {
        public ExplosionData explosion;

        public FloatRange validSeverities = new FloatRange(0, float.MaxValue);

        public HediffCompProperties_ExplodingRangedAttacks()
        {
            compClass = typeof(HediffComp_ExplodingRangedAttacks);
        }
    }
}

using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_ExplodingMeleeAttacks : HediffCompProperties
    {
        public ExplosionData explosion;

        public FloatRange validSeverities = new FloatRange(0, float.MaxValue);

        public HediffCompProperties_ExplodingMeleeAttacks()
        {
            compClass = typeof(HediffComp_ExplodingMeleeAttacks);
        }
    }
}

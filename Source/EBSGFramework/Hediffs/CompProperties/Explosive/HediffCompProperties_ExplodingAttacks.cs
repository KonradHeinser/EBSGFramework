using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_ExplodingAttacks : HediffCompProperties
    {
        public ExplosionData explosion;

        public FloatRange validSeverities = new FloatRange(0, float.MaxValue);

        public HediffCompProperties_ExplodingAttacks()
        {
            compClass = typeof(HediffComp_ExplodingAttacks);
        }
    }
}

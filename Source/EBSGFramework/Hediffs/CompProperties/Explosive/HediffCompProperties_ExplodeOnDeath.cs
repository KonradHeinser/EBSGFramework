using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_ExplodeOnDeath : HediffCompProperties
    {
        public ExplosionData explosion;

        public FloatRange validSeverities = new FloatRange(0, float.MaxValue);

        public HediffCompProperties_ExplodeOnDeath()
        {
            compClass = typeof(HediffComp_ExplodeOnDeath);
        }
    }
}

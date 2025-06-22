using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_ExplodeOnRemoval : HediffCompProperties
    {
        public bool allowDead = true;

        public ExplosionData explosion;

        public FloatRange validSeverities = new FloatRange(0, float.MaxValue);

        public HediffCompProperties_ExplodeOnRemoval()
        {
            compClass = typeof(HediffComp_ExplodeOnRemoval);
        }
    }
}

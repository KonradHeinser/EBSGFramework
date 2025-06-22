using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_ExplosiveRetaliation : HediffCompProperties
    {
        public int cooldownTicks = 60; // Set to once every other second to ensurethe first explosion is done before starting the next one

        public ExplosionData explosion;

        public FloatRange validSeverities = new FloatRange(0, float.MaxValue);

        public HediffCompProperties_ExplosiveRetaliation()
        {
            compClass = typeof(HediffComp_ExplosiveRetaliation);
        }
    }
}

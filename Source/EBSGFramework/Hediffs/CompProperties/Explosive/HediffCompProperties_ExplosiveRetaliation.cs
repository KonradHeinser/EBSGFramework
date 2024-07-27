namespace EBSGFramework
{
    public class HediffCompProperties_ExplosiveRetaliation : BurstHediffPropertiesBase
    {
        public int cooldownTicks = 60; // Set to once every other second to ensurethe first explosion is done before starting the next one

        public HediffCompProperties_ExplosiveRetaliation()
        {
            compClass = typeof(HediffComp_ExplosiveRetaliation);
        }
    }
}

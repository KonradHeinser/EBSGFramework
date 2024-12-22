namespace EBSGFramework
{
    public class HediffCompProperties_ExplodeOnRemoval : BurstHediffPropertiesBase
    {
        public bool allowDead = true;

        public HediffCompProperties_ExplodeOnRemoval()
        {
            compClass = typeof(HediffComp_ExplodeOnRemoval);
        }
    }
}

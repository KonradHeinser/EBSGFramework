using System;
namespace EBSGFramework
{
    public class HediffCompProperties_ExplodeWhenDamaged : BurstHediffPropertiesBase
    {
        public int cooldownTicks = 60; // Set to once every other second to ensurethe first explosion is done before starting the next one

        public HediffCompProperties_ExplodeWhenDamaged()
        {
            compClass = typeof(HediffComp_ExplodeWhenDamaged);
        }
    }
}

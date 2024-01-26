using System;
namespace EBSGFramework
{
    public class HediffCompProperties_ExplodeWhenDamaged : BurstHediffPropertiesBase
    {
        public HediffCompProperties_ExplodeWhenDamaged()
        {
            compClass = typeof(HediffComp_ExplodeOnDamaged);
        }
    }
}

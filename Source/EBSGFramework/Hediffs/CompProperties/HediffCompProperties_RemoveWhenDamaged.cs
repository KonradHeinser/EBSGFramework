using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_RemoveWhenDamaged : HediffCompProperties
    {
        public List<DamageDef> damageDefs = new List<DamageDef>();
        
        public CheckType checkType = CheckType.Required;

        public float amount = -1f;
        
        public FloatRange validAmounts = FloatRange.Zero;

        public HediffCompProperties_RemoveWhenDamaged()
        {
            compClass = typeof(HediffComp_RemoveWhenDamaged);
        }
    }
}
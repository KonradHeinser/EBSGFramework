using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_RemoveOnDamageTaken : HediffCompProperties
    {
        public List<DamageDef> damageDefs;

        public FloatRange amount = FloatRange.Zero;
        
        public CheckType checkType = CheckType.Required;

        public HediffCompProperties_RemoveOnDamageTaken()
        {
            compClass = typeof(HediffComp_RemoveOnDamageTaken);
        }
    }
}
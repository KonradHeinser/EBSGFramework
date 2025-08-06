using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_DamageBodyParts : HediffCompProperties
    {
        public List<BodyPartDef> bodyPartsToRemove;

        public int removeCount = 0;

        public List<PartToDamage> bodyPartsToDamage;

        public int damageCount = 0;

        public HediffCompProperties_DamageBodyParts()
        {
            compClass = typeof(HediffComp_DamageBodyParts);
        }
    }
}

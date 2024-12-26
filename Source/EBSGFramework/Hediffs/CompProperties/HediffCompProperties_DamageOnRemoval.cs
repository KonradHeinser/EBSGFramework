using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_DamageOnRemoval : HediffCompProperties
    {
        public DamageDef damage;

        public float amount = -1f;

        public float minHealthRemaining = 0f;

        public bool createFilth = true;

        public bool neverWhenDead = true;

        public List<BodyPartDef> bodyParts;

        public bool trulyRandomPart;

        public HediffCompProperties_DamageOnRemoval()
        {
            compClass = typeof(HediffComp_DamageOnRemoval);
        }
    }
}

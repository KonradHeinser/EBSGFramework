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

        public FloatRange severity = FloatRange.Zero;

        public HediffCompProperties_DamageOnRemoval()
        {
            compClass = typeof(HediffComp_DamageOnRemoval);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (damage == null)
                yield return "damage needs to be set.";
        }
    }
}

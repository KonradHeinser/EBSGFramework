using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_DamageOverTime : HediffCompProperties
    {
        public int tickInterval = 200;

        public DamageDef damage;

        public float damageAmount = 1f;

        public float armorPenetration = 0f;

        public bool createFilth = true;

        public bool damageAttachedPart = false;

        public List<BodyPartDef> bodyParts;

        public HediffCompProperties_DamageOverTime()
        {
            compClass = typeof(HediffComp_DamageOverTime);
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

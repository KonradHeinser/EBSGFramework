using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace EBSGFramework
{
    public class CompProperties_AbilityDamageOverTime : CompProperties_AbilityEffect
    {
        public int tickInterval = 60;

        public DamageDef damage;

        public float damageAmount = 1f;

        public float armorPenetration = 0f;

        public bool createFilth = true;

        public List<BodyPartDef> bodyParts;

        public CompProperties_AbilityDamageOverTime()
        {
            compClass = typeof(CompAbilityEffect_DamageOverTime);
        }
    }
}

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityDealDamage : CompProperties_AbilityEffect
    {
        public DamageDef def;
        
        public BodyPartHeight? height;

        public BodyPartDepth depth = BodyPartDepth.Undefined;

        public float amount = -1f;

        public float armorPenetration = -1f;

        public float chance = 1f;

        public StatEffects statEffects;

        public List<BodyPartDef> bodyParts;

        public CompProperties_AbilityDealDamage() 
        {
            compClass = typeof(CompAbilityEffect_DealDamage);
        }
    }
}

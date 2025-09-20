using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class CompProperties_AbilityDealDamage : CompProperties_AbilityEffect
    {
        public DamageDef def;

        public float amount = -1f;

        public float armorPenetration = -1f;

        public CompProperties_AbilityDealDamage() 
        {
            compClass = typeof(CompAbilityEffect_DealDamage);
        }
    }
}

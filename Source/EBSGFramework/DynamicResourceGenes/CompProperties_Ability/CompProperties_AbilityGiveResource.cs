using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityGiveResource : CompProperties_AbilityEffect
    {
        public float amount;

        public GeneDef mainResourceGene;

        public StatEffects statEffects;

        public bool applyTargetGainStat = true; // The stat specified in the resource gene itself

        public bool checkLimits = false; // If true, the target can't be targeted if the ability would take them below 0 or above the resource's Max

        public CompProperties_AbilityGiveResource()
        {
            compClass = typeof(CompAbilityEffect_GiveResource);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            if (amount > 0) yield return (string)("ResourceGain".Translate(mainResourceGene.resourceLabel) + ": ") + Mathf.RoundToInt(amount * 100f);
            else yield return (string)("ResourceDrain".Translate(mainResourceGene.resourceLabel) + ": ") + Mathf.RoundToInt(amount * -100f);
        }

    }
}

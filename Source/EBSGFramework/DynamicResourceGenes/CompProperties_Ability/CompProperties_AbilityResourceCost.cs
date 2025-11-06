using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityResourceCost : CompProperties_AbilityEffect
    {
        public float resourceCost;

        public GeneDef mainResourceGene;

        public StatDef costFactorStat;

        public bool checkMaximum = true; // Causes abilities to be uncastable if they would generate too much of the resource

        public CompProperties_AbilityResourceCost()
        {
            compClass = typeof(CompAbilityEffect_ResourceCost);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            if (resourceCost > 0) 
                yield return (string)("ResourceCost".Translate(mainResourceGene.resourceLabel).CapitalizeFirst() + ": ") + Mathf.RoundToInt(resourceCost * 100f);
            else
                yield return (string)("ResourceGain".Translate(mainResourceGene.resourceLabel).CapitalizeFirst() + ": ") + Mathf.RoundToInt(resourceCost * -100f);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityConvertResource : CompProperties_AbilityEffect
    {
        public float resourceCost;

        public float conversionEfficiency = 1f; // If this is 0.5, then for every 1 that the giver loses, the receiver gains 0.5

        public GeneDef giver;

        public GeneDef receiver;

        public CompProperties_AbilityConvertResource()
        {
            compClass = typeof(CompAbilityEffect_ConvertResource);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return (string)("ResourceCost".Translate(giver.resourceLabel) + ": ") + Mathf.RoundToInt(resourceCost * 100f);
            yield return (string)("ResourceGain".Translate(receiver.resourceLabel) + ": ") + Mathf.RoundToInt(resourceCost * -100f * conversionEfficiency);
        }
    }
}

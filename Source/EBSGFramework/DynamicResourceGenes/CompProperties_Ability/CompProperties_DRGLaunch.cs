using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_DRGLaunch : CompProperties_AbilityEffect
    {
        public int maxDistance = 9999; // Places a hard limit on distance that can't be surpassed even if the pawn has more resource

        public ThingDef skyfallerLeaving;

        public WorldObjectDef worldObject;

        public GeneDef mainResourceGene;

        public float baseCost = 0f;

        public float costPerTile = 0.1f;

        public CompProperties_DRGLaunch()
        {
            compClass = typeof(CompAbilityEffect_DRGLaunch);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            if (baseCost > 0) yield return (string)("ResourceCost".Translate(mainResourceGene.resourceLabel) + ": ") + Mathf.RoundToInt(baseCost * 100f);
            else yield return (string)("ResourceGain".Translate(mainResourceGene.resourceLabel) + ": ") + Mathf.RoundToInt(baseCost * -100f);

            if (costPerTile > 0) yield return (string)("ResourceCostPerTile".Translate(mainResourceGene.resourceLabel) + ": ") + Mathf.RoundToInt(costPerTile * 100f);
            else yield return (string)("ResourceGainPerTile".Translate(mainResourceGene.resourceLabel) + ": ") + Mathf.RoundToInt(costPerTile * -100f);
        }
    }
}

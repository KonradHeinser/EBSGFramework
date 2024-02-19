using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class IngestionOutcomeDoer_OffsetResource : IngestionOutcomeDoer
    {
        public float offset;

        public GeneDef mainResourceGene;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (mainResourceGene == null) Log.Error(ingested.Label + "is missing the mainResource gene, meaning it can't increase the resource level.");
            else
            {
                if (pawn.genes.GetGene(mainResourceGene) is ResourceGene resourceGene)
                {
                    ResourceGene.OffsetResource(pawn, offset * ingested.stackCount, resourceGene, resourceGene.def.GetModExtension<DRGExtension>(), true);
                }
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (ModsConfig.BiotechActive)
            {
                string text = ((offset >= 0f) ? "+" : string.Empty);
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, "Resource".Translate().CapitalizeFirst(), text + Mathf.RoundToInt(offset * 100f), "ResourceDesc".Translate(), 1000);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class Comp_DRGConsumable : ThingComp
    {
        public CompProperties_DRGConsumable Props => (CompProperties_DRGConsumable)props;

        public GeneLinker GetRelatedLinker(Pawn pawn)
        {
            if (pawn == null) return null;

            foreach (GeneLinker linker in Props.resourceOffsets)
                if (pawn.HasRelatedGene(linker.mainResourceGene))
                    return linker;

            return null;
        }

        public int NumberToConsume(Pawn pawn)
        {
            if (pawn == null) return 1;
            int num = 1;

            foreach (GeneLinker linker in Props.resourceOffsets)
                if (pawn.HasRelatedGene(linker.mainResourceGene) && pawn.genes.GetGene(linker.mainResourceGene) is ResourceGene resourceGene)
                    if (Mathf.FloorToInt(resourceGene.AmountMissing / linker.amount) > num)
                        num = Mathf.FloorToInt(resourceGene.AmountMissing / linker.amount);

            return num;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            bool flag = false;

            string text = "DRG_Consuming".Translate(parent.LabelShort);

            foreach (GeneLinker linker in Props.resourceOffsets)
                if (selPawn.HasRelatedGene(linker.mainResourceGene))
                {
                    flag = true;
                    if (linker.consumptionReportString != null)
                    {
                        text = linker.floatMenuString.Formatted(parent.LabelShort, parent);
                        break;
                    }
                }

            if (flag)
            {
                if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
                {
                    yield return new FloatMenuOption(text + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                    yield break;
                }

                if (!selPawn.CanReserve(parent))
                {
                    yield return new FloatMenuOption(text + ": " + "Reserved".Translate().CapitalizeFirst(), null);
                    yield break;
                }

                yield return new FloatMenuOption(text, delegate
                {
                    Job job = JobMaker.MakeJob(EBSGDefOf.DRG_Consume, parent);
                    job.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
                });
            }
        }
    }
}

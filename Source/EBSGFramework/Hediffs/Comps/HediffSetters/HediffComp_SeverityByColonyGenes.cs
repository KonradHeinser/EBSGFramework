using Verse;
using System.Linq;
using RimWorld.Planet;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_SeverityByColonyGenes : HediffComp
    {
        public HediffCompProperties_SeverityByColonyGenes Props => (HediffCompProperties_SeverityByColonyGenes)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.IsHashIntervalTick(100) || parent.pawn.Faction == null) return;
            Pawn pawn = parent.pawn;

            int count = 0;

            if (pawn.Map != null)
            {
                count = CheckGeneCount(Pawn.Map.mapPawns.AllPawns.Where((Pawn p) => p.Faction != null && p.Faction == pawn.Faction).ToList());
            }
            else
            {
                Caravan caravan = pawn.GetCaravan();
                if (caravan != null)
                {
                    count = CheckGeneCount(caravan.PawnsListForReading.Where((Pawn p) => p.Faction != null && p.Faction == pawn.Faction).ToList());
                }
                else
                {
                    if (pawn.genes != null)
                    {
                        if (Props.gene != null)
                        {
                            if (pawn.HasRelatedGene(Props.gene))
                                count++;
                        }
                        if (!Props.genes.NullOrEmpty())
                            if (Props.mustHaveAllGenes)
                            {
                                if (EBSGUtilities.PawnHasAllOfGenes(pawn, Props.genes)) count++;
                            }
                            else if (EBSGUtilities.PawnHasAnyOfGenes(pawn, out var gene, Props.genes)) count++;
                    }
                }
            }

            if (count == 0 && !Props.removeWhenNoGenes) parent.Severity = 0.1f;
            else parent.Severity = count;
        }

        public int CheckGeneCount(List<Pawn> pawns)
        {
            if (pawns.NullOrEmpty()) return 0;

            int count = 0;

            foreach (Pawn pawn in pawns)
            {
                if (pawn.genes == null) continue;

                if (Props.gene != null)
                {
                    if (pawn.HasRelatedGene(Props.gene))
                        count++;
                }
                if (!Props.genes.NullOrEmpty())
                    if (Props.mustHaveAllGenes)
                    {
                        if (EBSGUtilities.PawnHasAllOfGenes(pawn, Props.genes)) count++;
                    }
                    else if (EBSGUtilities.PawnHasAnyOfGenes(pawn, out var gene, Props.genes)) count++;
            }

            return count;
        }
    }
}

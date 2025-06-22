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
            if (!Pawn.IsHashIntervalTick(600) || Pawn.Faction == null) return;

            int count = 0;

            if (Pawn.Map != null)
                count = CheckGeneCount(Pawn.Map.mapPawns.AllPawns.Where((Pawn p) => p.Faction != null && p.Faction == Pawn.Faction).ToList());
            else
            {
                Caravan caravan = Pawn.GetCaravan();
                if (caravan != null)
                    count = CheckGeneCount(caravan.PawnsListForReading.Where((Pawn p) => p.Faction != null && p.Faction == Pawn.Faction).ToList());
                else if (Pawn.genes != null)
                {
                    if (Props.gene != null && Pawn.HasRelatedGene(Props.gene))
                        count++;

                    if (!Props.genes.NullOrEmpty())
                        if (Props.mustHaveAllGenes)
                        {
                            if (Pawn.PawnHasAllOfGenes(Props.genes))
                                count++;
                        }
                        else if (Pawn.PawnHasAnyOfGenes(out var gene, Props.genes))
                            count++;
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
                if (Pawn.genes == null) 
                    continue;

                if (Props.gene != null && Pawn.HasRelatedGene(Props.gene))
                    count++;

                if (!Props.genes.NullOrEmpty())
                    if (Props.mustHaveAllGenes)
                    {
                        if (Pawn.PawnHasAllOfGenes(Props.genes)) count++;
                    }
                    else if (Pawn.PawnHasAnyOfGenes(out var gene, Props.genes)) count++;
            }

            return count;
        }
    }
}

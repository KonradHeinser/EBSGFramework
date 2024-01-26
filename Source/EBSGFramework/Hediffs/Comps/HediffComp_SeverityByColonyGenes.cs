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
                count = CheckGeneCount(Pawn.Map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction != null && p.Faction == pawn.Faction).ToList());
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
                        if (Props.gene != null && pawn.genes.HasGene(Props.gene)) count++;
                        else if (Props.mustHaveAllGenes && !Props.genes.NullOrEmpty())
                        {
                            bool flag = false;
                            foreach (GeneDef gene in Props.genes)
                            {
                                if (!pawn.genes.HasGene(gene))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag) count++;
                        }
                        else if (EBSGUtilities.PawnHasAnyOfGenes(pawn, Props.genes)) count++;
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
                if (Props.gene != null && pawn.genes.HasGene(Props.gene)) count++;
                else if (Props.mustHaveAllGenes && !Props.genes.NullOrEmpty())
                {
                    bool flag = false;
                    foreach (GeneDef gene in Props.genes)
                    {
                        if (!pawn.genes.HasGene(gene))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) count++;
                }
                else if (EBSGUtilities.PawnHasAnyOfGenes(pawn, Props.genes)) count++;
            }

            return count;
        }
    }
}

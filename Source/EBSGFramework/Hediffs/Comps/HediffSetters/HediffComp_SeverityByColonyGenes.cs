using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByColonyGenes : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByColonyGenes Props => (HediffCompProperties_SeverityByColonyGenes)props;

        protected override void SetSeverity()
        {
            base.SetSeverity();

            int count = 0;

            if (Pawn.Faction == null)
            {
                if (Pawn.CheckPawnGenes(Props.gene, Props.genes, Props.mustHaveAllGenes))
                    count = 1;
            }
            else if (Pawn.Map != null)
                count = CheckGeneCount(Pawn.Map.mapPawns.AllPawns.Where((Pawn p) => p.Faction != null && p.Faction == Pawn.Faction).ToList());
            else
            {
                Caravan caravan = Pawn.GetCaravan();
                if (caravan != null)
                    count = CheckGeneCount(caravan.PawnsListForReading.Where((Pawn p) => p.Faction != null && p.Faction == Pawn.Faction).ToList());
                else if (Pawn.CheckPawnGenes(Props.gene, Props.genes, Props.mustHaveAllGenes))
                    count = 1;
            }

            if (count == 0 && !Props.removeWhenNoGenes) 
                parent.Severity = 0.1f;
            else 
                parent.Severity = count;

            ticksToNextCheck = 2500;
        }

        public int CheckGeneCount(List<Pawn> pawns)
        {
            if (pawns.NullOrEmpty()) return 0;

            int count = 0;

            foreach (Pawn pawn in pawns)
                if (pawn.CheckPawnGenes(Props.gene, Props.genes, Props.mustHaveAllGenes))
                    count++;

            return count;
        }
    }
}

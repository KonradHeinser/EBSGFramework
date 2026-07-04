using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemovedBy: HediffComp
    {
        public HediffCompProperties_RemovedBy Props => (HediffCompProperties_RemovedBy)props;

        public override bool CompShouldRemove => remove;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            CheckRemoval();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (Pawn.genes?.GenesListForReading.NullOrEmpty() != false)
                return;
            if (geneCount != Pawn.genes.GenesListForReading.Count || Pawn.IsHashIntervalTick(2500, delta))
                CheckRemoval();
        }

        private int geneCount = -1;
        
        private bool remove;
        
        private void CheckRemoval()
        {
            remove = Pawn.PawnHasAnyOfGenes(out _, Props.genes);
            geneCount = Pawn.genes?.GenesListForReading?.Count ?? 0;
        }
    }
}
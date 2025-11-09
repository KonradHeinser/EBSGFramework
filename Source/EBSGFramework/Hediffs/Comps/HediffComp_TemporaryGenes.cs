using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_TemporaryGenes : HediffComp
    {
        public HediffCompProperties_TemporaryGenes Props => (HediffCompProperties_TemporaryGenes)props;

        public List<GeneDef> addedGenes;

        private FloatRange? currentRange;

        private float prevSeverity;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (addedGenes == null)
                addedGenes = new List<GeneDef>();

            currentRange = null;

            if (Props.genesAtSeverities.NullOrEmpty())
            {
                Log.Error(Def + " doesn't have any lists to use in HediffCompProperties_TemporaryGenes. Deleting hediff to avoid more errors.");
                parent.pawn.health.RemoveHediff(parent);
                return;
            }

            CheckGenes();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (prevSeverity != parent.Severity && currentRange?.ValidValue(parent.Severity) != true)
                CheckGenes();
        }

        private void CheckGenes()
        {
            prevSeverity = parent.Severity;
            foreach (GenesAtSeverity geneSet in Props.genesAtSeverities)
            {
                if (geneSet.validSeverity.ValidValue(parent.Severity))
                {
                    if (EBSGUtilities.EquivalentGeneLists(new List<GeneDef>(addedGenes), new List<GeneDef>(geneSet.genes))) break;
                    parent.pawn.RemoveGenesFromPawn(addedGenes);
                    addedGenes.Clear();
                    addedGenes = parent.pawn.AddGenesToPawn(geneSet.xenogenes, geneSet.genes);
                    currentRange = geneSet.validSeverity;
                    break;
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            parent.pawn.RemoveGenesFromPawn(addedGenes);
            addedGenes.Clear();
        }

        public override void CompExposeData()
        {
            Scribe_Collections.Look(ref addedGenes, "EBSG_AddedGenes");
        }
    }
}

using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_AlterXenotype : HediffComp
    {
        HediffCompProperties_AlterXenotype Props => (HediffCompProperties_AlterXenotype)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            AlterXenotype();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            AlterXenotype();
        }

        private void AlterXenotype()
        {
            if (!Props.severities.Includes(parent.Severity)) return;
            if (Props.setXenotype)
                Pawn.genes.SetXenotype(Props.xenotype);
            else
            {
                Pawn.genes.SetXenotypeDirect(Props.xenotype);
                bool isGermline = Props.xenotype.inheritable;
                List<Gene> genesListForReading = isGermline ? Pawn.genes.Endogenes : Pawn.genes.Xenogenes;
                List<Gene> genesListToRemove = new List<Gene>();
                for (int i = 0; i < Props.xenotype.genes.Count; i++)
                {
                    if (!genesListForReading.NullOrEmpty())
                    {
                        foreach (Gene gene in genesListForReading)
                            if (Props.xenotype.genes[i].ConflictsWith(gene.def))
                                genesListToRemove.Add(gene);
                        foreach (Gene gene in genesListToRemove)
                        {
                            genesListForReading.Remove(gene);
                            Pawn.genes.RemoveGene(gene);
                        }
                    }
                    Pawn.genes.AddGene(Props.xenotype.genes[i], !isGermline);
                }
            }

            if (Pawn.Spawned && Props.filth != null)
                FilthMaker.TryMakeFilth(Pawn.Position, Pawn.Map, Props.filth, Pawn.LabelIndefinite(), Props.filthCount.RandomInRange);

            if (Props.sendMessage)
                Messages.Message("EBSG_XenotypeApplied".Translate(Pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);

            Pawn.health.RemoveHediff(parent);
        }
    }
}

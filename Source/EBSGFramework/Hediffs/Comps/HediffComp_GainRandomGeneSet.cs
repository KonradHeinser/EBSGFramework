using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_GainRandomGeneSet : HediffComp
    {
        private HediffCompProperties_GainRandomGeneSet Props => (HediffCompProperties_GainRandomGeneSet)props;
        public List<GeneDef> genesToAdd = new List<GeneDef>();
        public List<GeneDef> remainingGenes = new List<GeneDef>();
        public List<GeneDef> alwaysAddedGenes = new List<GeneDef>();
        public List<GeneDef> alwaysRemovedGenes = new List<GeneDef>();
        public int delayTicks;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.delayTicks < 10) // To avoid potential issues of having multiple things happen to a pawn at spawn, wait several ticks
            {
                delayTicks = 10;
            }
            else
            {
                delayTicks = Props.delayTicks;
            }
            alwaysAddedGenes = Props.alwaysAddedGenes;
            alwaysRemovedGenes = Props.alwaysRemovedGenes;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (delayTicks < 0) return;
            if (delayTicks == 0)
            {
                GenerateGenes();
                delayTicks--;
            }
            else if (parent.Severity >= Props.minSeverity && parent.Severity <= Props.maxSeverity)
            {
                delayTicks--;
            }
        }

        private void GenerateGenes()
        {
            Pawn_GeneTracker currentGenes = parent.pawn.genes;
            bool reverseInheritence = false;
            bool inheritGenes = Props.inheritable;

            foreach (Gene currentGene in currentGenes.GenesListForReading) // Puts genes into a list that's easier to check
            {
                remainingGenes.Add(currentGene.def);
            }

            // Select a geneSet to be added
            if (!Props.geneSets.NullOrEmpty())
            {
                float totalWeight = 0;
                foreach (RandomXenoGenes xenoGeneSet in Props.geneSets)
                {
                    totalWeight += xenoGeneSet.weightOfGeneSet;
                }

                double randomNumber = new Random().NextDouble() * totalWeight;
                foreach (RandomXenoGenes xenoGeneSet in Props.geneSets)
                {
                    if (randomNumber <= xenoGeneSet.weightOfGeneSet)
                    {
                        genesToAdd = xenoGeneSet.geneSet;
                        reverseInheritence = xenoGeneSet.reverseInheritence;
                        break;
                    }
                    randomNumber -= xenoGeneSet.weightOfGeneSet;
                }
            }

            if (reverseInheritence) inheritGenes = !inheritGenes;

            // Get rid of specific current genes if removeGenesFromOtherLists still true
            if (Props.removeGenesFromOtherLists && !Props.geneSets.NullOrEmpty())
            {
                foreach (RandomXenoGenes xenoGeneSet in Props.geneSets) // For each list
                {
                    foreach (GeneDef geneDef in xenoGeneSet.geneSet) // For each gene in the list
                    {
                        if (remainingGenes.Contains(geneDef))   // If the easy list from above contains it, get rid of it
                        {
                            currentGenes.RemoveGene(parent.pawn.genes.GetGene(geneDef));
                            remainingGenes.Remove(geneDef);
                        }

                    }
                }
            }
            else if (!Props.geneSets.NullOrEmpty())
            {
                foreach (RandomXenoGenes xenoGeneSet in Props.geneSets) // For each list
                {
                    if (xenoGeneSet.alwaysRemoveGenes)
                    {
                        foreach (GeneDef geneDef in xenoGeneSet.geneSet) // For each gene in the list
                        {
                            if (remainingGenes.Contains(geneDef))   // If the easy list from above contains it, get rid of it
                            {
                                currentGenes.RemoveGene(parent.pawn.genes.GetGene(geneDef));
                                remainingGenes.Remove(geneDef);
                            }

                        }
                    }
                }
            }

            // Add genes if they don't exist, and remove genes if they are on the alway remove list
            if (!alwaysAddedGenes.NullOrEmpty())
            {
                foreach (GeneDef geneDef in alwaysAddedGenes)
                {
                    if (!remainingGenes.Contains(geneDef))
                    {
                        currentGenes.AddGene(geneDef, !inheritGenes);
                        remainingGenes.Add(geneDef);
                    }
                }
            }
            
            if (!genesToAdd.NullOrEmpty())
            {

                foreach (GeneDef geneDef in genesToAdd)
                {
                    if (!remainingGenes.Contains(geneDef))
                    {
                        currentGenes.AddGene(geneDef, !inheritGenes);
                        remainingGenes.Add(geneDef);
                    }
                }
            }

            if (!alwaysRemovedGenes.NullOrEmpty())
            {
                foreach (GeneDef geneDef in alwaysRemovedGenes)
                {
                    if (remainingGenes.Contains(geneDef))
                    {
                        currentGenes.RemoveGene(parent.pawn.genes.GetGene(geneDef));
                        remainingGenes.Remove(geneDef);
                    }
                }
            }

            parent.pawn.genes = currentGenes;

            // Wrap things up
            if (parent.pawn.Faction == Faction.OfPlayer) // If the pawn is in the player faction, give a message based on what is most relevant to the player.
            {
                if (!Props.geneSets.NullOrEmpty()) Messages.Message("Random genes successfully generated!", MessageTypeDefOf.NeutralEvent, false);
                else if (!alwaysAddedGenes.NullOrEmpty()) Messages.Message("Genes successfully added to pawn!", MessageTypeDefOf.NeutralEvent, false);
                else if (!alwaysRemovedGenes.NullOrEmpty()) Messages.Message("Genes successfully removed from pawn!", MessageTypeDefOf.NeutralEvent, false);
                else Messages.Message("This hediff successfully processed that the mod dev gave me NOTHING to work with. Why?", MessageTypeDefOf.NeutralEvent, false);
            }
            if (parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def) != null && Props.removeHediffAfterwards)
            {
                parent.pawn.health.RemoveHediff(parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def));
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref delayTicks, "EBSG_GeneSetDelayTicks", 10);
        }
    }
}

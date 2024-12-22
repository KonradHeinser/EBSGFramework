using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompAbilityEffect_AlterXenotype : CompAbilityEffect
    {
        public new CompProperties_AbilityAlterXenotype Props => (CompProperties_AbilityAlterXenotype)props;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!(target.Thing is Pawn pawn) || pawn.genes == null)
            {
                if (throwMessages)
                    Messages.Message("EBSG_RequireGenes".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            return base.Valid(target, throwMessages);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = target.Pawn;
            Pawn caster = parent.pawn;

            if (!Props.useCasterXeno)
                EBSGUtilities.AlterXenotype(pawn, Props.xenotypes, Props.filth, Props.filthCount, Props.setXenotype, Props.sendMessage);
            else
            {
                if (!caster.genes.UniqueXenotype)
                    EBSGUtilities.AlterXenotype(pawn, caster.genes.Xenotype, Props.filth, Props.filthCount, Props.setXenotype, Props.sendMessage);
                else
                {
                    if (Props.setXenotype)
                    {
                        pawn.genes.Endogenes.RemoveWhere((arg) => arg.def.endogeneCategory != EndogeneCategory.HairColor && arg.def.endogeneCategory != EndogeneCategory.Melanin);
                        pawn.genes.ClearXenogenes();
                        if (!caster.genes.Endogenes.NullOrEmpty())
                            foreach (Gene gene in caster.genes.Endogenes)
                                if (gene.def.endogeneCategory != EndogeneCategory.HairColor && gene.def.endogeneCategory != EndogeneCategory.Melanin)
                                    pawn.genes.AddGene(gene.def, true);

                        if (!caster.genes.Xenogenes.NullOrEmpty())
                            foreach (Gene gene in caster.genes.Xenogenes)
                                pawn.genes.AddGene(gene.def, false);
                    }
                    else
                    {
                        List<Gene> genesListForReading = new List<Gene>(pawn.genes.GenesListForReading);
                        List<Gene> genesListToRemove = new List<Gene>();

                        if (!caster.genes.Endogenes.NullOrEmpty())
                            foreach (Gene gene in caster.genes.Endogenes)
                            {
                                if (!genesListForReading.NullOrEmpty())
                                {
                                    foreach (Gene g in genesListForReading)
                                        if (gene.def.ConflictsWith(g.def) || gene.def.prerequisite?.ConflictsWith(g.def) == true)
                                            genesListToRemove.Add(g);

                                    foreach (Gene r in genesListToRemove)
                                    {
                                        genesListForReading.Remove(gene);
                                        pawn.genes.RemoveGene(r);
                                    }
                                }
                                pawn.genes.AddGene(gene.def, true);
                            }

                        if (!caster.genes.Xenogenes.NullOrEmpty())
                            foreach (Gene gene in caster.genes.Xenogenes)
                            {
                                if (!genesListForReading.NullOrEmpty())
                                {
                                    foreach (Gene g in genesListForReading)
                                        if (gene.def.ConflictsWith(g.def) || gene.def.prerequisite?.ConflictsWith(g.def) == true)
                                            genesListToRemove.Add(g);

                                    foreach (Gene r in genesListToRemove)
                                    {
                                        genesListForReading.Remove(gene);
                                        pawn.genes.RemoveGene(r);
                                    }
                                }
                                pawn.genes.AddGene(gene.def, true);
                            }
                    }

                    if (pawn.Spawned && Props.filth != null)
                        FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, Props.filth, pawn.LabelIndefinite(), Props.filthCount.RandomInRange);

                    if (Props.sendMessage)
                        Messages.Message("EBSG_XenotypeApplied".Translate(pawn.LabelShortCap), MessageTypeDefOf.NeutralEvent, false);

                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                }
            }
        }
    }
}

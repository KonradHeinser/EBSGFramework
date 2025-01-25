using Verse;
using System.Collections.Generic;
using RimWorld;
using System;

namespace EBSGFramework
{
    public class Hediff_Dependency : HediffWithComps
    {
        public ChemicalDef chemical;

        private Gene_Dependency cachedDependencyGene;

        public override string LabelBase => LinkedGene.Label;

        public string AssignedLabel => GetLabel();

        public int cachedGeneCount = 0;

        public GeneDef linkedGene;

        public IDGExtension cachedExtension;

        public string GetLabel()
        {
            if (Extension != null && Extension.dependencyLabel != null)
            {
                return Extension.dependencyLabel;
            }
            return chemical.label;
        }

        public override bool ShouldRemove => LinkedGene?.Active != true;

        public float FirstNotableStageSeverity => Extension.minSatisfySeverity ?? def.stages[1].minSeverity - 0.1f;

        public bool ShouldSatisfy => Severity >= FirstNotableStageSeverity;

        public Gene_Dependency LinkedGene
        {
            get
            {
                if (pawn.genes == null) pawn.health.RemoveHediff(this);
                if (cachedDependencyGene == null || pawn.genes.GenesListForReading.Count != cachedGeneCount)
                {
                    List<Gene> genesListForReading = pawn.genes.GenesListForReading;
                    cachedGeneCount = genesListForReading.Count;
                    foreach (Gene gene in genesListForReading)
                        if (gene is Gene_Dependency gene_Dependency)
                        {
                            if (chemical != null)
                            {
                                if (gene_Dependency.def.chemical == chemical)
                                {
                                    cachedDependencyGene = gene_Dependency;
                                    break;
                                }
                            }
                            else if (gene_Dependency.def == linkedGene)
                            {
                                cachedDependencyGene = gene_Dependency;
                                break;
                            }
                        }
                }
                return cachedDependencyGene;
            }
        }

        public IDGExtension Extension
        {
            get
            {
                if (LinkedGene != null && cachedExtension == null)
                {
                    cachedExtension = LinkedGene.def.GetModExtension<IDGExtension>();
                }
                return cachedExtension;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (LinkedGene == null) pawn.health.RemoveHediff(this);
        }

        public override string TipStringExtra
        {
            get
            {
                string text = base.TipStringExtra;
                if (LinkedGene != null && !def.comps.NullOrEmpty())
                {
                    if (!text.NullOrEmpty())
                    {
                        text += "\n\n";
                    }
                    if (Extension != null && Extension.descriptionOverride != null)
                    {
                        text += Extension.descriptionOverride;
                    }
                    else
                    {
                        float severityPerDay = 0;
                        foreach (HediffCompProperties compProps in def.comps)
                        {
                            if (compProps is HediffCompProperties_SeverityPerDay severityComp)
                            {
                                if (severityComp.severityPerDay > 0) severityPerDay = severityComp.severityPerDay;
                                else severityPerDay = severityComp.severityPerDayRange.Average;
                            }
                        }
                        if (severityPerDay > 0 && !def.stages.NullOrEmpty())
                        {
                            bool firstFlag = true;
                            text += " " + "EBSG_DependencyNeedDurationDescriptionBase".Translate(AssignedLabel, pawn.Named("PAWN")).Resolve();
                            foreach (HediffStage stage in def.stages)
                            {
                                if (stage.minSeverity <= FirstNotableStageSeverity) continue;
                                double days = Math.Round((double)(stage.minSeverity / severityPerDay), 1);

                                string experienceLabel = AssignedLabel + " ";
                                if (stage.label != null) experienceLabel += stage.label;
                                if (stage.overrideLabel != null) experienceLabel = stage.overrideLabel;

                                bool deathStage = false;
                                if (!stage.capMods.NullOrEmpty())
                                {
                                    foreach (PawnCapacityModifier capMod in stage.capMods)
                                    {
                                        if (EBSGUtilities.LethalCapacities.Contains(capMod.capacity))
                                        {
                                            if (capMod.setMax <= 0 || capMod.postFactor <= 0)
                                            {
                                                deathStage = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (deathStage)
                                {
                                    text += " " + "EBSG_DependencyNeedDurationDescriptionDeath".Translate(days, pawn.Named("PAWN")).Resolve();
                                    break;
                                }
                                if (firstFlag)
                                {
                                    text += " " + "EBSG_DependencyNeedDurationDescriptionFirst".Translate(AssignedLabel, days, experienceLabel, pawn.Named("PAWN")).Resolve();
                                    firstFlag = false;
                                }
                                else text += " " + "EBSG_DependencyNeedDurationDescription".Translate(days, experienceLabel, pawn.Named("PAWN")).Resolve();
                            }
                        }
                        else
                        {
                            if (severityPerDay <= 0) Log.Error(def + " isn't using the HediffCompProperties_SeverityPerDay, which is required for the automatic description generation.");
                            else Log.Error(def + " doesn't have any stages, which are required for the automatic description generator");
                        }
                    }
                    text += "\n\n";
                    text += "EBSG_LastIngestedDurationAgo".Translate(AssignedLabel, (Find.TickManager.TicksGame - LinkedGene.lastIngestedTick).ToStringTicksToPeriod().Named("DURATION")).Resolve();
                }
                else
                {
                    if (def.comps.NullOrEmpty()) Log.Error(def + " doesn't have any comps, which makes it really hard to do things.");
                    else Log.Error("I don't even know how you got this error, but here's the def attached to it: " + def);
                }
                return text;
            }
        }

        public override bool TryMergeWith(Hediff other)
        {
            if (!(other is Hediff_Dependency hediff_Dependency)) return false;

            if (chemical != null)
            {
                if (hediff_Dependency.chemical == chemical) return base.TryMergeWith(other);
            }
            else if (hediff_Dependency.AssignedLabel == AssignedLabel) return base.TryMergeWith(other);

            return false;
        }

        public override void CopyFrom(Hediff other)
        {
            base.CopyFrom(other);
            if (other is Hediff_Dependency hediff_Dependency)
            {
                chemical = hediff_Dependency.chemical;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref chemical, "chemical");
            Scribe_Defs.Look(ref linkedGene, "linkedGene");
        }
    }
}

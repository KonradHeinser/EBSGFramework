using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class Gene_Dependency : Gene
    {
        public int lastIngestedTick;

        public IDGExtension cachedExtension;

        public IDGExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    cachedExtension = def.GetModExtension<IDGExtension>();
                }
                return cachedExtension;
            }
        }

        public Hediff_Dependency LinkedHediff
        {
            get
            {
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    if (hediffs[i] is Hediff_Dependency hediff_Dependency)
                    {
                        if (def.chemical != null) if (hediff_Dependency.chemical == def.chemical) return hediff_Dependency;
                        if (hediff_Dependency.AssignedLabel == Extension.dependencyLabel) return hediff_Dependency;
                    }
                }
                return null;
            }
        }

        public override void PostAdd()
        {
            if (!ModLister.CheckBiotech("Chemical dependency")) return;
            base.PostAdd();

            HediffAdder.HediffAdding(pawn, this);

            if (Extension == null)
            {
                Log.Error(def + " is missing the IDGExtension. Removing the gene to avoid more errors.");
                pawn.genes.RemoveGene(this);
            }
            if (def.chemical == null && Extension.dependencyLabel == null)
            {
                Log.Error(def + " is not using a chemical and doesn't have a dependency label. Removing the gene to avoid more errors.");
                pawn.genes.RemoveGene(this);
            }
            if (Extension.dependencyHediff == null)
            {
                Log.Error(def + " doesn't have an assigned hediff. Removing the gene to avoid more errors.");
                pawn.genes.RemoveGene(this);
            }

            if (def.chemical != null)
            {
                if (def.chemical.addictionHediff != null)
                {
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def.chemical.addictionHediff);
                    if (firstHediffOfDef != null)
                    {
                        Log.Message("Removing hediff A");
                        pawn.health.RemoveHediff(firstHediffOfDef);
                    }
                }
            }
            AddDependencyHediff();
            lastIngestedTick = Find.TickManager.TicksGame;
        }

        public override void PostRemove()
        {
            Hediff_Dependency linkedHediff = LinkedHediff;
            if (LinkedHediff != null)
            {
                Log.Message("Removing hediff B");
                pawn.health.RemoveHediff(linkedHediff);
            }
            base.PostRemove();
        }

        private void AddDependencyHediff()
        {
            Hediff hediff = HediffMaker.MakeHediff(Extension.dependencyHediff, pawn);
            if (hediff is Hediff_Dependency hediff_Dependency)
            {
                if (def.chemical != null) hediff_Dependency.chemical = def.chemical;
                else hediff_Dependency.linkedGene = def;

                hediff_Dependency.Severity = hediff_Dependency.def.initialSeverity;

                pawn.health.AddHediff(hediff_Dependency);
            }
            else
            {
                Log.Error(def + "'s linked hediff is not using the EBSGFramework.Hediff_Dependency class. Removing the gene to avoid more errors.");
                pawn.genes.RemoveGene(this);
            }
        }

        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            if (def.chemical != null)
            {
                if (thing.def.thingCategories.NullOrEmpty() || thing.def.thingCategories.Contains(ThingCategoryDefOf.Drugs))
                {
                    CompDrug compDrug = thing.TryGetComp<CompDrug>();
                    if (compDrug != null && compDrug.Props.chemical == def.chemical)
                    {
                        Reset();
                    }
                }
            }
            else
            {
                CompIngredients ingredients = thing.TryGetComp<CompIngredients>();
                bool flag = false;
                if (!flag && !Extension.validThings.NullOrEmpty())
                {
                    foreach (ThingDef thingDef in Extension.validThings)
                    {
                        if (thing.def == thingDef)
                        {
                            flag = true;
                            break;
                        }
                        if (Extension.checkIngredients && ingredients != null && !ingredients.ingredients.NullOrEmpty())
                        {
                            if (ingredients.ingredients.Contains(thingDef))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                if (!flag && !Extension.validCategories.NullOrEmpty())
                {
                    if (!flag && !thing.def.thingCategories.NullOrEmpty())
                    {
                        foreach (ThingCategoryDef thingCategory in Extension.validCategories)
                        {
                            if (thing.def.thingCategories.Contains(thingCategory))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag && Extension.checkIngredients && ingredients != null && !ingredients.ingredients.NullOrEmpty())
                    {
                        foreach (ThingDef ingredient in ingredients.ingredients)
                        {
                            if (!ingredient.thingCategories.NullOrEmpty())
                            {
                                foreach (ThingCategoryDef thingCategory in Extension.validCategories)
                                {
                                    if (ingredient.thingCategories.Contains(thingCategory))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            if (flag) break;
                        }
                    }
                }

                if (flag) Reset();
            }
        }

        public override void Reset()
        {
            Hediff_Dependency linkedHediff = LinkedHediff;
            if (linkedHediff != null)
            {
                linkedHediff.Severity = linkedHediff.def.initialSeverity;
            }
            else AddDependencyHediff();

            lastIngestedTick = Find.TickManager.TicksGame;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastIngestedTick, "lastIngestedTick", 0);
        }
    }
}

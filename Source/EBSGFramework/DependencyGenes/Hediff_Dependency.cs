using Verse;
using System.Collections.Generic;
using RimWorld;
using System;
using Verse.AI;

namespace EBSGFramework
{
    public class Hediff_Dependency : HediffWithComps
    {
        public ChemicalDef chemical;

        private Gene_Dependency cachedDependencyGene;

        public override string LabelBase => LinkedGene.Label;

        public string AssignedLabel => GetLabel();

        public int cachedGeneCount = 0;

        private GeneDef linkedGene;

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
                            bool deathStage = false;
                            foreach (HediffStage stage in def.stages)
                            {
                                if (stage.minSeverity <= FirstNotableStageSeverity) continue;
                                double days = Math.Round((double)(stage.minSeverity / severityPerDay), 1);

                                string experienceLabel = AssignedLabel + " ";
                                if (stage.label != null) experienceLabel += stage.label;
                                if (stage.overrideLabel != null) experienceLabel = stage.overrideLabel;

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
                            if (!deathStage && IsLethal)
                                text += " " + "EBSG_DependencyNeedDurationDescriptionDeath".Translate(Math.Round((double)(def.lethalSeverity / severityPerDay), 1), pawn.Named("PAWN")).Resolve();
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

        // The pawn in question is the getter, but not always the consumer
        public Thing FindIngestibleFor(Pawn pawn)
        {
            ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (IngestibleValidator(pawn, innerContainer[i]))
                {
                    return innerContainer[i];
                }
            }

            if (pawn.IsColonist && pawn.Map?.mapPawns?.SpawnedColonyAnimals.NullOrEmpty() == false)
                foreach (Pawn spawnedColonyAnimal in pawn.Map.mapPawns.SpawnedColonyAnimals)
                    foreach (Thing item in spawnedColonyAnimal.inventory.innerContainer)
                        if (IngestibleValidator(pawn, item) && !spawnedColonyAnimal.IsForbidden(pawn) && pawn.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
                            return item;

            if (chemical != null)
            {
                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => IngestibleValidator(pawn, x));
                if (thing != null)
                    return thing;
            }
            else
            {
                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => IngestibleValidator(pawn, x));
                if (thing != null)
                    return thing;
            }

            if (Extension != null)
            {
                if (!Extension.validThings.NullOrEmpty())
                {
                    foreach (ThingDef thingDef in Extension.validThings)
                    {
                        Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(thingDef), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => IngestibleValidator(pawn, t));
                        if (thing != null)
                            return thing;
                    }
                    if (Extension.checkIngredients)
                    {
                        List<Thing> things = pawn.Map.listerThings.AllThings.FindAll((Thing t) => LinkedGene.ValidIngest(t));
                        if (!things.NullOrEmpty())
                        {
                            Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, things, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => IngestibleValidator(pawn, t));
                            if (thing != null)
                                return thing;
                        }
                    }
                }
                if (!Extension.validCategories.NullOrEmpty())
                {
                    List<Thing> things = pawn.Map.listerThings.AllThings.FindAll((Thing t) => t.IngestibleNow && !t.IsForbidden(pawn) && pawn.CanReserve(t) && CheckCategories(t));
                    if (!things.NullOrEmpty())
                    {
                        Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, things, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => IngestibleValidator(pawn, t));
                        if (thing != null)
                            return thing;
                    }
                }
            }
            return null;
        }

        private bool CheckCategories(Thing thing)
        {
            if (!thing.def.thingCategories.NullOrEmpty())
                foreach (ThingCategoryDef thingCategory in thing.def.thingCategories)
                    if (Extension.validCategories.Contains(thingCategory))
                        return true;

            CompIngredients ingredients = thing.TryGetComp<CompIngredients>();

            if (Extension.checkIngredients && ingredients?.ingredients.NullOrEmpty() == false)
            {
                if (Extension.validCategories.Contains(ThingCategoryDefOf.MeatRaw) &&
                 FoodUtility.GetFoodKind(thing) == FoodKind.Meat)
                    return true;
                if (Extension.validCategories.Contains(ThingCategoryDefOf.PlantFoodRaw) &&
                    FoodUtility.GetFoodKind(thing) == FoodKind.NonMeat)
                    return true;

                foreach (ThingDef ingredient in ingredients.ingredients)
                    if (!ingredient.thingCategories.NullOrEmpty())
                        foreach (ThingCategoryDef ingredientCategory in ingredient.thingCategories)
                            if (Extension.validCategories.Contains(ingredientCategory))
                                return true;
            }

            return false;
        }

        private bool IngestibleValidator(Pawn pawn, Thing item)
        {
            if (!item.IngestibleNow || !pawn.CanReserve(item) && item.IsForbidden(pawn)) return false;

            if (chemical != null)
            {
                if (!item.def.IsDrug)
                {
                    return false;
                }
                if (item.Spawned && (!pawn.CanReserve(item) || item.IsForbidden(pawn) || !item.IsSociallyProper(pawn)))
                {
                    return false;
                }
                CompDrug compDrug = item.TryGetComp<CompDrug>();
                if (compDrug == null || compDrug.Props.chemical == null || compDrug.Props.chemical != chemical)
                {
                    return false;
                }
                if (pawn.drugs != null && !pawn.drugs.CurrentPolicy[item.def].allowedForAddiction && (!pawn.InMentalState || pawn.MentalStateDef.ignoreDrugPolicy))
                {
                    return false;
                }
                return true;
            }
            if (LinkedGene.ValidIngest(item)) return true;
            return false;
        }

    }
}

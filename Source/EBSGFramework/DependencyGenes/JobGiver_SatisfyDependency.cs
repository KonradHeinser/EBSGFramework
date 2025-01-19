using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobGiver_SatisfyDependency : ThinkNode_JobGiver
    {
        private static readonly List<Hediff_Dependency> tmpDependencies = new List<Hediff_Dependency>();

        public override float GetPriority(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive)
            {
                return 0f;
            }
            if (pawn.health.hediffSet.hediffs.Any((Hediff x) => ShouldSatify(x)))
            {
                return 9.25f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            tmpDependencies.Clear();
            if (!ModsConfig.BiotechActive) return null;

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            if (!hediffs.NullOrEmpty())
            {
                foreach (Hediff hediff in hediffs)
                {
                    if (hediff is Hediff_Dependency dependency && ShouldSatify(hediff))
                    {
                        tmpDependencies.Add(dependency);
                    }
                }
            }
            if (tmpDependencies.NullOrEmpty()) return null;
            tmpDependencies.SortBy((Hediff_Dependency x) => 0f - x.Severity);
            foreach (Hediff_Dependency hediff in tmpDependencies)
            {
                Thing thing = FindIngestibleFor(pawn, hediff);
                if (thing != null)
                {
                    tmpDependencies.Clear();
                    Pawn pawn2 = (thing.ParentHolder as Pawn_InventoryTracker)?.pawn;
                    if (pawn2 != null && pawn2 != pawn)
                    {
                        Job takeJob = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, thing, pawn2);
                        takeJob.count = 1;
                        return takeJob;
                    }
                    Job IngestJob = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                    IngestJob.count = 1;
                    CompDrug compDrug = thing.TryGetComp<CompDrug>();
                    if (compDrug != null && pawn.drugs != null)
                    {
                        DrugPolicyEntry drugPolicyEntry = pawn.drugs.CurrentPolicy[thing.def];
                        int num = pawn.inventory.innerContainer.TotalStackCountOfDef(thing.def) - IngestJob.count;
                        if (drugPolicyEntry.allowScheduled && num <= 0)
                        {
                            IngestJob.takeExtraIngestibles = drugPolicyEntry.takeToInventory;
                        }
                    }
                    return IngestJob;
                }
            }
            tmpDependencies.Clear();
            return null;
        }

        private bool ShouldSatify(Hediff hediff)
        {
            if (!(hediff is Hediff_Dependency hediff_ChemicalDependency))
                return false;
            return hediff_ChemicalDependency.ShouldSatisfy;
        }

        private Thing FindIngestibleFor(Pawn pawn, Hediff_Dependency dependency)
        {
            ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (IngestibleValidator(pawn, dependency, innerContainer[i]))
                {
                    return innerContainer[i];
                }
            }

            if (pawn.IsColonist && pawn.Map != null)
            {
                foreach (Pawn spawnedColonyAnimal in pawn.Map.mapPawns.SpawnedColonyAnimals)
                {
                    foreach (Thing item in spawnedColonyAnimal.inventory.innerContainer)
                    {
                        if (IngestibleValidator(pawn, dependency, item) && !spawnedColonyAnimal.IsForbidden(pawn) && pawn.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
                        {
                            return item;
                        }
                    }
                }
            }

            if (dependency.chemical != null)
            {
                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => IngestibleValidator(pawn, dependency, x));
                if (thing != null)
                {
                    return thing;
                }
            }
            else
            {
                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => IngestibleValidator(pawn, dependency, x));
                if (thing != null)
                {
                    return thing;
                }
            }
            if (dependency.Extension != null)
            {
                IDGExtension extension = dependency.Extension;
                if (!extension.validThings.NullOrEmpty())
                {
                    foreach (ThingDef thingDef in extension.validThings)
                    {
                        Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(thingDef), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => IngestibleValidator(pawn, dependency, t));
                        if (thing != null) return thing;
                    }
                    if (extension.checkIngredients)
                    {
                        List<Thing> things = pawn.Map.listerThings.AllThings.FindAll((Thing t) => dependency.LinkedGene.ValidIngest(t));
                        if (!things.NullOrEmpty())
                        {
                            Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, things, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => IngestibleValidator(pawn, dependency, t));
                            if (thing != null) return thing;
                        }
                    }
                }
                if (!extension.validCategories.NullOrEmpty())
                {
                    List<Thing> things = pawn.Map.listerThings.AllThings.FindAll((Thing t) => t.IngestibleNow && !t.IsForbidden(pawn) && pawn.CanReserve(t) && CheckCategories(t, extension));
                    if (!things.NullOrEmpty())
                    {
                        Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, things, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => IngestibleValidator(pawn, dependency, t));
                        if (thing != null) return thing;
                    }
                }
            }
            return null;
        }

        private bool CheckCategories(Thing thing, IDGExtension extension)
        {
            if (!thing.def.thingCategories.NullOrEmpty())
                foreach (ThingCategoryDef thingCategory in thing.def.thingCategories)
                    if (extension.validCategories.Contains(thingCategory))
                        return true;

            CompIngredients ingredients = thing.TryGetComp<CompIngredients>();
            
            if (extension.checkIngredients && ingredients?.ingredients.NullOrEmpty() == false)
            {
                if (extension.validCategories.Contains(ThingCategoryDefOf.MeatRaw) &&
                 FoodUtility.GetFoodKind(thing) == FoodKind.Meat)
                    return true;
                if (extension.validCategories.Contains(ThingCategoryDefOf.PlantFoodRaw) &&
                    FoodUtility.GetFoodKind(thing) == FoodKind.NonMeat)
                    return true;

                foreach (ThingDef ingredient in ingredients.ingredients)
                    if (!ingredient.thingCategories.NullOrEmpty())
                        foreach (ThingCategoryDef ingredientCategory in ingredient.thingCategories)
                            if (extension.validCategories.Contains(ingredientCategory)) 
                                return true;
            }

            return false;
        }

        private bool IngestibleValidator(Pawn pawn, Hediff_Dependency dependency, Thing item)
        {
            if (!item.IngestibleNow || !pawn.CanReserve(item) && item.IsForbidden(pawn)) return false;

            if (dependency.chemical != null)
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
                if (compDrug == null || compDrug.Props.chemical == null || compDrug.Props.chemical != dependency.chemical)
                {
                    return false;
                }
                if (pawn.drugs != null && !pawn.drugs.CurrentPolicy[item.def].allowedForAddiction && (!pawn.InMentalState || pawn.MentalStateDef.ignoreDrugPolicy))
                {
                    return false;
                }
                return true;
            }
            if (dependency.LinkedGene.ValidIngest(item)) return true;
            return false;
        }
    }
}

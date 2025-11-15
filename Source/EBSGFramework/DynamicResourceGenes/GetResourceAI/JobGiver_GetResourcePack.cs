using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobGiver_GetResourcePack : ThinkNode_JobGiver
    {
        private static EBSGCache_Component cache;

        public static EBSGCache_Component Cache
        {
            get
            {
                if (cache == null)
                    cache = Current.Game.GetComponent<EBSGCache_Component>();

                if (cache != null && cache.loaded)
                    return cache;
                return null;
            }
        }

        private static float? cachedResourcePackResourceGain;

        public static float ResourcePackResourceGain(Pawn pawn, List<ThingDef> resourcePacks, ResourceGene resource, Thing resourcePack = null)
        {
            if (!cachedResourcePackResourceGain.HasValue || resourcePack != null)
            {
                if (!ModsConfig.BiotechActive)
                    cachedResourcePackResourceGain = 0f;
                else
                {
                    Thing carriedThing = pawn.carryTracker.CarriedThing;
                    foreach (ThingDef thing in resourcePacks)
                    {
                        if (thing.HasComp<Comp_DRGConsumable>())
                        {
                            CompProperties_DRGConsumable comp = null;

                            foreach (CompProperties compProp in thing.comps)
                            {
                                if (compProp is CompProperties_DRGConsumable)
                                {
                                    comp = (CompProperties_DRGConsumable)compProp;
                                    break;
                                }
                            }
                            if (comp.resourceOffsets.NullOrEmpty())
                            {
                                Log.Error(thing.defName + " is using CompProperties_DRGConsumable, but doesn't have any genes listed.");
                                cachedResourcePackResourceGain = 0f;
                            }
                            else
                                foreach (GeneLinker geneLinker in comp.resourceOffsets)
                                    if (geneLinker.mainResourceGene == resource.def)
                                    {
                                        cachedResourcePackResourceGain = geneLinker.amount * (resource.statFactor != null ? pawn.GetStatValue(resource.statFactor) : 1f);
                                        break;
                                    }
                        }
                        else if (!(thing.ingestible?.outcomeDoers?.FirstOrDefault((IngestionOutcomeDoer x) => x is IngestionOutcomeDoer_OffsetResource ix && (ix).mainResourceGene == resource.def) is IngestionOutcomeDoer_OffsetResource ingestionOutcomeDoer_OffsetResource))
                            cachedResourcePackResourceGain = 0f;
                        else
                            cachedResourcePackResourceGain = ingestionOutcomeDoer_OffsetResource.offset;

                        // Checks if inventory has the thing because if so it'll be the default anyway. In the event that the target is already known, that's used instead
                        if (resourcePack == null)
                        {
                            if (carriedThing != null && carriedThing.def == thing)
                            {
                                return cachedResourcePackResourceGain.Value;
                            }
                            for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
                            {
                                if (pawn.inventory.innerContainer[i].def == thing)
                                {
                                    return cachedResourcePackResourceGain.Value;
                                }
                            }
                        }
                        else if (resourcePack.def == thing)
                        {
                            return cachedResourcePackResourceGain.Value;
                        }
                    }
                }
            }
            return 0f; // If never found an item in the inventory, then give up
        }

        public static void ResetStaticData()
        {
            cachedResourcePackResourceGain = null;
        }

        public override float GetPriority(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || pawn.genes?.GetFirstGeneOfType<ResourceGene>() == null
                || (Cache != null && Cache.dynamicResourceGenes.NullOrEmpty())) return 0f;
            return 9.1f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || (Cache != null && Cache.dynamicResourceGenes.NullOrEmpty())) return null;

            ResourceGene resourceGene = pawn.genes?.GetFirstGeneOfType<ResourceGene>(); // Verifies that any resource gene exists
            if (resourceGene == null)
                return null;

            List<ResourceGene> resourcesPresent = new List<ResourceGene>(); // Creates list of all resource genes
            if (Cache != null)
            {
                foreach (GeneDef gene in Cache.dynamicResourceGenes)
                    if (pawn.HasRelatedGene(gene))
                        resourcesPresent.Add(pawn.genes.GetGene(gene) as ResourceGene);
            }
            else
                foreach (Gene gene in pawn.genes?.GenesListForReading)
                    if (gene.def.HasModExtension<DRGExtension>() && gene.def.GetModExtension<DRGExtension>().isMainGene) resourcesPresent.Add((ResourceGene)gene);

            foreach (ResourceGene resource in resourcesPresent) // Check each resource gene, and the moment a viable one appears, return job
            {
                if (!resource.ShouldConsumeResourceNow()) continue;
                DRGExtension extension = resource.def.GetModExtension<DRGExtension>();
                if (resource.resourcePacksAllowed && !extension.resourcePacks.NullOrEmpty())
                {
                    Thing resourcePack = GetResourcePack(pawn, extension.resourcePacks);
                    int num = Mathf.FloorToInt((resource.Max - resource.Value) / ResourcePackResourceGain(pawn, extension.resourcePacks, resource, resourcePack));
                    if (num > 0 && resourcePack != null)
                        if (resourcePack.HasComp<Comp_DRGConsumable>())
                        {
                            Job job = JobMaker.MakeJob(EBSGDefOf.DRG_Consume, resourcePack);
                            job.count = Mathf.Min(resourcePack.stackCount, num);
                            job.ingestTotalCount = true;
                            return job;
                        }
                        else
                        {
                            Job job = JobMaker.MakeJob(JobDefOf.Ingest, resourcePack);
                            job.count = Mathf.Min(resourcePack.stackCount, num);
                            job.ingestTotalCount = true;
                            return job;
                        }
                }
            }
            return null;
        }

        private Thing GetResourcePack(Pawn pawn, List<ThingDef> resourcePacks)
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            foreach (ThingDef thingDef in resourcePacks)
            {
                if (carriedThing != null && carriedThing.def == thingDef)
                    return carriedThing;
                for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
                    if (pawn.inventory.innerContainer[i].def == thingDef)
                        return pawn.inventory.innerContainer[i];

                Thing returnThing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(thingDef), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t) && !t.IsForbidden(pawn));
                if (returnThing != null) return returnThing;
            }
            return null;
        }

        // Left just in case I add this in the future
        public static AcceptanceReport CanFeedOnPrisoner(Pawn bloodfeeder, Pawn prisoner, float bloodlossAmount = 0.49999f)
        {
            if (prisoner.WouldDieFromAdditionalBloodLoss(bloodlossAmount))
                return "CannotFeedOnWouldKill".Translate(prisoner.Named("PAWN"));

            if (!prisoner.IsPrisonerOfColony || !prisoner.guest.PrisonerIsSecure || !prisoner.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Bloodfeed) || prisoner.IsForbidden(bloodfeeder) || !bloodfeeder.CanReserveAndReach(prisoner, PathEndMode.OnCell, bloodfeeder.NormalMaxDanger()) || prisoner.InAggroMentalState)
                return false;

            return true;
        }

        private Pawn GetPrisoner(Pawn pawn)
        {
            return (Pawn)GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.mapPawns.PrisonersOfColonySpawned, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => t is Pawn prisoner && CanFeedOnPrisoner(pawn, prisoner).Accepted);
        }
    }
}

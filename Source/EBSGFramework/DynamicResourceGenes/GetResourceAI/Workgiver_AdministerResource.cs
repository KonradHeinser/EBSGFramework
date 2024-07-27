using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Workgiver_AdministerResource : WorkGiver_Scanner
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

        private const float MinLevelForFeedingResourceUnforced = 0.25f;

        private const float ResourcePctMax = 0.95f;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (Cache != null && Cache.dynamicResourceGenes.NullOrEmpty())
                return false;

            if (!(t is Pawn pawn2) || pawn2 == pawn || !FeedPatientUtility.ShouldBeFed(pawn2)) return false;
            ResourceGene resourceGene = pawn2.genes?.GetFirstGeneOfType<ResourceGene>();
            if (resourceGene == null) return false;

            List<ResourceGene> resourcesPresent = new List<ResourceGene>(); // Creates list of all resource genes
            if (Cache != null)
            {
                foreach (GeneDef gene in Cache.dynamicResourceGenes)
                    if (EBSGUtilities.HasRelatedGene(pawn2, gene))
                        resourcesPresent.Add(pawn2.genes.GetGene(gene) as ResourceGene);
            }
            else
                foreach (Gene gene in pawn2.genes?.GenesListForReading)
                {
                    if (gene.def.HasModExtension<DRGExtension>() && gene.def.GetModExtension<DRGExtension>().isMainGene) resourcesPresent.Add((ResourceGene)gene);
                }

            bool flag = false;
            bool flag2 = false;
            foreach (ResourceGene resource in resourcesPresent)
            {
                // Check resource values and reservation
                if (resource.ValuePercent >= 0.95f || (!forced && resource.Value >= resource.targetValue) || !pawn.CanReserve(t, 1, -1, null, forced)
                        || !resource.ShouldConsumeResourceNow()) continue;
                // Check resource pack permission
                if (!resource.resourcePacksAllowed)
                {
                    flag = true;
                    continue;
                }

                // Try to find viable resources for consumption
                if (FindViableResource(resource.def.GetModExtension<DRGExtension>().resourcePacks, pawn) == null)
                {
                    flag2 = true;
                    continue;
                }
                return true; // If there's a viable item, return true

            }
            if (flag2) JobFailReason.Is("NoIngredients".Translate()); // If one of the resources passed all checks except the find viable one, mention it
            else if (flag) JobFailReason.Is("NotAllowedResource".Translate()); // Otherwise, if it's just due to a lack of permissions, make it this one
            return false; // If there are no viable items, return false
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (Cache != null && Cache.dynamicResourceGenes.NullOrEmpty())
                return null;

            if (!(t is Pawn pawn2)) return null;

            ResourceGene resourceGene = pawn2.genes?.GetFirstGeneOfType<ResourceGene>();
            if (resourceGene == null) return null;

            List<ResourceGene> resourcesPresent = new List<ResourceGene>(); // Creates list of all resource genes
            foreach (Gene gene in pawn2.genes?.GenesListForReading)
            {
                if (gene.def.HasModExtension<DRGExtension>() && gene.def.GetModExtension<DRGExtension>().isMainGene) resourcesPresent.Add((ResourceGene)gene);
            }
            foreach (ResourceGene resource in resourcesPresent)
            {
                Thing thing = FindViableResource(resource.def.GetModExtension<DRGExtension>().resourcePacks, pawn);
                if (thing != null)
                {
                    if (thing.HasComp<Comp_DRGConsumable>())
                    {
                        Job job = JobMaker.MakeJob(EBSGDefOf.DRG_FeedPatient, thing, pawn2);
                        job.count = 1;
                        return job;
                    }
                    else
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, thing, pawn2);
                        job.count = 1;
                        return job;
                    }

                }
            }
            return null;
        }

        private Thing FindViableResource(List<ThingDef> resourcePacks, Pawn pawn)
        {
            foreach (ThingDef thingDef in resourcePacks)
            {
                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack));
                if (thing != null) return thing;
            }
            return null;
        }
    }
}

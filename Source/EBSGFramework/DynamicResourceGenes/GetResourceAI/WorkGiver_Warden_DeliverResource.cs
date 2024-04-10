using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class WorkGiver_Warden_DeliverResource : WorkGiver_Warden
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

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModsConfig.BiotechActive || Cache != null && Cache.dynamicResourceGenes.NullOrEmpty())
                return null;

            Pawn prisoner = (Pawn)t;
            ResourceGene resourceGene = prisoner.genes?.GetFirstGeneOfType<ResourceGene>(); // Checks if there's even a resource gene present
            if (resourceGene == null || !ShouldTakeCareOfPrisoner(pawn, prisoner) || !prisoner.guest.CanBeBroughtFood || !prisoner.Position.IsInPrisonCell(prisoner.Map) || WardenFeedUtility.ShouldBeFed(prisoner))
            {
                // Checks all global reasons to not feed the prisoner resource packs. If there's no global reason, then there's no reason to find a local reason
                return null;
            }
            List<ResourceGene> resourcesPresent = new List<ResourceGene>(); // Creates list of all resource genes
            foreach (Gene gene in prisoner.genes?.GenesListForReading)
                if (gene.def.HasModExtension<DRGExtension>() && gene.def.GetModExtension<DRGExtension>().isMainGene) resourcesPresent.Add((ResourceGene)gene);

            foreach (ResourceGene gene in resourcesPresent) // Go through all of the present resources to find any that need resupply
            {
                if (!gene.resourcePacksAllowed || !gene.ShouldConsumeResourceNow()) return null; // Check if consumption is even needed
                DRGExtension extension = gene.def.GetModExtension<DRGExtension>();
                if (extension.resourcePacks.NullOrEmpty() || ResourcePackAlreadyAvailableFor(prisoner, extension.resourcePacks)) return null; // Check if there's already a resouce pack
                foreach (ThingDef thingDef in extension.resourcePacks)
                {
                    Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack) && pack.GetRoom() != prisoner.GetRoom());
                    if (thing == null) continue; // If the individual item doesn't exist, try the next one
                    if (thing.HasComp<Comp_DRGConsumable>())
                    {
                        Job job = JobMaker.MakeJob(EBSGDefOf.DRG_Deliver, thing, prisoner);
                        job.count = 1;
                        job.targetC = RCellFinder.SpotToChewStandingNear(prisoner, thing);
                        return job;
                    }
                    else
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.DeliverFood, thing, prisoner);
                        job.count = 1;
                        job.targetC = RCellFinder.SpotToChewStandingNear(prisoner, thing);
                        return job;
                    }
                }
            }
            return null; // If none of the existing resources require packs/have packs available, no job for you
        }

        private bool ResourcePackAlreadyAvailableFor(Pawn prisoner, List<ThingDef> resourcePacks)
        {
            foreach (ThingDef thing in resourcePacks)
            {
                if (prisoner.carryTracker.CarriedCount(thing) > 0 || prisoner.inventory.Count(thing) > 0)
                    return true;
                Room room = prisoner.GetRoom();
                if (room != null)
                {
                    List<Region> regions = room.Regions;
                    for (int i = 0; i < regions.Count; i++)
                        if (regions[i].ListerThings.ThingsOfDef(thing).Count > 0)
                            return true;
                }
            }
            return false;
        }
    }
}

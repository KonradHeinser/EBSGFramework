using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class WorkGiver_Warden_DeliverDependency : WorkGiver_Warden
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

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModsConfig.BiotechActive) 
                return null;
            if (Cache?.idgGenes.NullOrEmpty() != false) 
                return null;
            if (!ShouldTakeCareOfPrisoner(pawn, t, forced))
                return null;

            Pawn p = t as Pawn;
            if (p?.Position.IsInPrisonCell(p.Map) != true)
                return null;
            if (WardenFeedUtility.ShouldBeFed(p)) 
                return null;
            if (!p.NeedToSatisfyIDG(out var tmpDependencies))
                return null;

            foreach (var hediff in tmpDependencies)
            {
                // If a dependency on the list is already being satisfied, we want to wait to see if others may also be satisfied at the same time
                if (IngestibleAlreadyAvailable(pawn, hediff))
                    return null;
                Thing thing = hediff.FindIngestibleFor(pawn);
                if (thing != null)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.DeliverFood, thing, p);
                    job.count = 1;
                    job.targetC = RCellFinder.SpotToChewStandingNear(p, thing);
                    return job;
                }
            }

            return null;
        }

        private bool IngestibleAlreadyAvailable(Pawn p, Hediff_Dependency dependency)
        {
            if (p.carryTracker.CarriedThing != null && dependency.LinkedGene?.ValidIngest(p.carryTracker.CarriedThing) == true)
                return true;
            foreach (var t in p.inventory.innerContainer)
            {
                if (dependency.LinkedGene.ValidIngest(t))
                    return true;
            }
            Room room = p.GetRoom();
            
            if (room != null)
            {
                List<Region> regions = room.Regions;

                if (dependency.Extension?.validThings?.Contains(ThingDefOf.MealNutrientPaste) == true &&
                    regions.SelectMany(t => t.ListerThings.AllThings).Any(t => t is Building_NutrientPasteDispenser d && d.CanDispenseNow))
                    return true;
                    
                return regions.SelectMany(t => t.ListerThings.AllThings).Any(t => dependency.LinkedGene.ValidIngest(t));
            }
            return false;
        }
    }
}

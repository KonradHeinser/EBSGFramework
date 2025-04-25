using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class Workgiver_AdministerDependency : WorkGiver_Scanner
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

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModsConfig.BiotechActive)
                return false;
            if (Cache?.idgGenes.NullOrEmpty() != false)
                return false;

            Pawn p = t as Pawn;
            if (!pawn.CanReserve(p, ignoreOtherReservations: forced))
                return false;
            if (!FeedPatientUtility.ShouldBeFed(p))
                return false;
            if (!p.NeedToSatisfyIDG(out var tmpDependencies))
                return false;

            foreach (var hediff in tmpDependencies)
            {
                if (hediff.FindIngestibleFor(pawn) != null)
                    return true;
            }
            JobFailReason.Is("EBSG_NoValidItem".Translate());
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn p = t as Pawn;
            if (!p.NeedToSatisfyIDG(out var tmpDependencies))
                return null;

            foreach (var hediff in tmpDependencies)
            {
                Thing thing = hediff.FindIngestibleFor(pawn);
                if (thing != null)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, thing, p);
                    job.count = 1;
                    return job;
                }
            }

            return null;
        }
    }
}

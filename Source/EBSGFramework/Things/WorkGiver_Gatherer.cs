﻿using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class WorkGiver_Gatherer : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(def.GetModExtension<EBSGExtension>().relatedThing);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserveAndReach(t, PathEndMode, Danger.Deadly))
            {
                return false;
            }

            if (!t.HasComp<CompGathererSpot>())
            {
                JobFailReason.Is("EBSG_MissingGathererComp".Translate(t.Label));
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(EBSGDefOf.EBSG_GathererJob, t);
        }
    }
}

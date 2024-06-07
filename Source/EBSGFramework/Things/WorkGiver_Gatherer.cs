using RimWorld;
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
                return false;

            if (!t.HasComp<CompGathererSpot>())
                JobFailReason.Is("EBSG_MissingGathererComp".Translate(t.Label));

            if (t.TryGetComp<CompGathererSpot>().ViableOptions.NullOrEmpty())
                JobFailReason.Is("EBSG_NoValidTarget".Translate(pawn.LabelShort, t.Label) + " " + "EBSG_TryAnotherLocation".Translate());

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (def.GetModExtension<EBSGExtension>().relatedJob != null)
                return JobMaker.MakeJob(def.GetModExtension<EBSGExtension>().relatedJob, t);
            return JobMaker.MakeJob(EBSGDefOf.EBSG_GathererJob, t);
        }
    }
}

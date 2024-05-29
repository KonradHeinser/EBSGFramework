using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class WorkGiver_Gather : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(def.GetModExtension<EBSGExtension>().relatedThing);

        public WorkGiver_Gather()
        {
        }
    }
}

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class WorkGiver_HaulNutritionToFoodCasket : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(def.GetModExtension<EBSGExtension>().relatedThing);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
                return false;

            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                return false;

            if (t.IsBurning())
                return false;

            if (!(t is Building_SleepCasket sleepCasket))
                return false;

            if (sleepCasket.NutritionNeeded > sleepCasket.MaxStorage * 0.25f)
            {
                if (FindNutrition(pawn, sleepCasket).Thing == null)
                {
                    JobFailReason.Is("NoFood".Translate());
                    return false;
                }
                return true;
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_SleepCasket sleepCasket))
                return null;

            if (sleepCasket.NutritionNeeded > 0f)
            {
                ThingCount thingCount = FindNutrition(pawn, sleepCasket);
                if (thingCount.Thing != null)
                {
                    Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
                    job.count = Mathf.Min(job.count, thingCount.Count);
                    return job;
                }
            }
            return null;
        }

        private ThingCount FindNutrition(Pawn pawn, Building_SleepCasket casket)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
            if (thing == null)
                return default(ThingCount);

            int b = Mathf.CeilToInt(casket.NutritionNeeded / thing.GetStatValue(StatDefOf.Nutrition));
            return new ThingCount(thing, Mathf.Min(thing.stackCount, b));
            bool Validator(Thing x)
            {
                if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
                    return false;

                if (!casket.CanAcceptNutrition(x))
                    return false;

                if (x.def.GetStatValueAbstract(StatDefOf.Nutrition) > casket.NutritionNeeded)
                    return false;

                return true;
            }
        }
    }
}

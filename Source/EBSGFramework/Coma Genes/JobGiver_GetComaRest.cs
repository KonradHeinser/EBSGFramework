using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace EBSGFramework
{
    public class JobGiver_GetComaRest : ThinkNode_JobGiver
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

        public override float GetPriority(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || Cache?.ComaNeedsExist() != true)
                return 0f;

            Lord lord = pawn.GetLord();
            if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
                return 0f;

            Need_ComaGene comaNeed = pawn.needs?.TryGetNeed<Need_ComaGene>();
            if (comaNeed == null) return 0f;

            if (comaNeed.CurLevelPercentage > 0.05f) // If the found coma gene doesn't need replenishing, check to make sure there aren't multiple coma needs for some reason
            {
                comaNeed = null;
                foreach (Need need in pawn.needs.AllNeeds)
                    if (need is Need_ComaGene && comaNeed.CurLevelPercentage <= 0.05f)
                    {
                        comaNeed = need as Need_ComaGene;
                        break;
                    }

                if (comaNeed == null)
                    return 0f;
            }
            return 7.75f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || Cache?.ComaNeedsExist() != true)
                return null;

            Need_ComaGene comaNeed = pawn.needs?.TryGetNeed<Need_ComaGene>();
            if (comaNeed == null) return null;

            if (comaNeed.CurLevelPercentage > 0.05f) // If the found coma gene doesn't need replenishing, check to make sure there aren't multiple coma needs for some reason
            {
                comaNeed = null;
                foreach (Need need in pawn.needs.AllNeeds)
                    if (need is Need_ComaGene && comaNeed.CurLevelPercentage <= 0.05f)
                    {
                        comaNeed = need as Need_ComaGene;
                        break;
                    }

                if (comaNeed == null)
                    return null;
            }
            Lord lord = pawn.GetLord();
            Building_Bed building_Bed = null;
            if ((lord == null || lord.CurLordToil == null || lord.CurLordToil.AllowRestingInBed) && !pawn.IsWildMan() && (!pawn.InMentalState || pawn.MentalState.AllowRestingInBed))
            {
                Pawn_RopeTracker roping = pawn.roping;
                if (roping == null || !roping.IsRoped)
                    building_Bed = RestUtility.FindBedFor(pawn);
            }

            if (building_Bed != null)
                return JobMaker.MakeJob(comaNeed.ComaGene.Extension.relatedJob, building_Bed);

            return JobMaker.MakeJob(comaNeed.ComaGene.Extension.relatedJob, FindGroundSleepSpotFor(pawn));
        }

        private IntVec3 FindGroundSleepSpotFor(Pawn pawn)
        {
            Map map = pawn.Map;
            IntVec3 position = pawn.Position;
            for (int i = 0; i < 2; i++)
            {
                int radius = ((i == 0) ? 4 : 12);
                if (CellFinder.TryRandomClosewalkCellNear(position, map, radius, out var result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander))
                    return result;
            }
            return CellFinder.RandomClosewalkCellNearNotForbidden(pawn, 4);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobGiver_PawnNeedCharger : ThinkNode_JobGiver
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

        public static Building_PawnNeedCharger ClosestViableCharger(Pawn pawn)
        {
            List<Thing> buildings = pawn.Map.spawnedThings.ToList().Where((Thing t) => t.def.thingClass == typeof(Building_PawnNeedCharger)).ToList();

            if (buildings.NullOrEmpty())
                return null;

            List<string> alreadyCheckedBuildingDefs = new List<string>();

            foreach (Thing thing in buildings)
            {
                bool flag = false; // Flag for if this building would take away a need or resource that is already low
                bool flag2 = false; // Flag for if this building is viable
                if (alreadyCheckedBuildingDefs.Contains(thing.def.defName)) continue;
                alreadyCheckedBuildingDefs.Add(thing.def.defName);
                EBSGExtension extension = thing.def.GetModExtension<EBSGExtension>();

                if (extension == null) continue;

                if (!extension.needOffsetsPerHour.NullOrEmpty() && pawn.needs != null && !pawn.needs.AllNeeds.NullOrEmpty())
                {
                    foreach (NeedOffset need in extension.needOffsetsPerHour)
                    {
                        if (need.need == null) continue; // Random need stuff should just be ignored
                        Need currentNeed = pawn.needs.TryGetNeed(need.need);

                        if (currentNeed != null)
                        {
                            if (need.offset <= 0 && currentNeed.CurLevel < 0.5)
                            {
                                flag = true;
                                break; // If this building would start draining a need that is already low, avoid this building
                            }
                            flag2 |= (need.offset > 0 && currentNeed.CurLevel < 0.3);
                        }
                    }
                }

                if (!flag && !extension.resourceOffsetsPerHour.NullOrEmpty() && pawn.genes != null && !pawn.genes.GenesListForReading.NullOrEmpty() && pawn.genes?.GetFirstGeneOfType<ResourceGene>() != null)
                {
                    foreach (GeneEffect geneEffect in extension.resourceOffsetsPerHour)
                    {
                        if (pawn.HasRelatedGene(geneEffect.gene) && pawn.genes.GetGene(geneEffect.gene) is ResourceGene resourceGene)
                        {
                            if (geneEffect.offset <= 0 && resourceGene.Value < resourceGene.targetValue)
                            {
                                flag = true;
                                break;
                            }
                            flag2 |= (geneEffect.offset > 0 && resourceGene.Value < resourceGene.targetValue);
                        }
                    }
                }

                if (!flag && flag2) // If this building does not specifically take from a low need/resource and will replenish a need/resource that is low, use it
                    return thing as Building_PawnNeedCharger;
            }

            return null;
        }

        public override float GetPriority(Pawn pawn)
        {
            if (Cache?.NeedRechargerJob() != true || pawn.Map == null || pawn.CurJobDef == EBSGDefOf.EBSG_NeedCharge) return 0f;
            return 9.1f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (Cache?.NeedRechargerJob() != true || pawn.Map == null) return null;

            Building_PawnNeedCharger building = ClosestViableCharger(pawn);

            if (building != null)
            {
                Job job = JobMaker.MakeJob(EBSGDefOf.EBSG_NeedCharge, building);
                job.overrideFacing = Rot4.South;
                return job;
            }

            return null;
        }
    }
}

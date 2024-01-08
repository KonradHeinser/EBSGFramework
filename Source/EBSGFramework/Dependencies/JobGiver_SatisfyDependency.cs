using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace EBSGFramework
{
    public class JobGiver_SatisfyDependency : ThinkNode_JobGiver
    {
        private static readonly List<Hediff_Dependency> tmpDependencies = new List<Hediff_Dependency>();

        public override float GetPriority(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive)
            {
                return 0f;
            }
            if (pawn.health.hediffSet.hediffs.Any((Hediff x) => ShouldSatify(x)))
            {
                return 9.25f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            tmpDependencies.Clear();
            if (!ModsConfig.BiotechActive) return null;

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            if (!hediffs.NullOrEmpty())
            {
                foreach (Hediff hediff in hediffs)
                {
                    if (hediff is Hediff_Dependency dependency && ShouldSatify(hediff))
                    {
                        tmpDependencies.Add(dependency);
                    }
                }
            }
            if (tmpDependencies.NullOrEmpty()) return null;

            tmpDependencies.SortBy((Hediff_Dependency x) => 0f - x.Severity);
            foreach (Hediff_Dependency hediff in tmpDependencies)
            {
                Thing thing = FindIngestibleFor(pawn, hediff);
                if (thing != null)
                {
                    tmpDependencies.Clear();
                    Pawn pawn2 = (thing.ParentHolder as Pawn_InventoryTracker)?.pawn;
                    if (pawn2 != null && pawn2 != pawn)
                    {
                        Job takeJob = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, thing, pawn2);
                        takeJob.count = 1;
                        return takeJob;
                    }
                    Job IngestJob = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                    IngestJob.count = Mathf.Min(thing.stackCount, thing.def.ingestible.maxNumToIngestAtOnce, 10);
                    CompDrug compDrug = thing.TryGetComp<CompDrug>();
                    if (compDrug != null && pawn.drugs != null)
                    {
                        DrugPolicyEntry drugPolicyEntry = pawn.drugs.CurrentPolicy[thing.def];
                        int num = pawn.inventory.innerContainer.TotalStackCountOfDef(thing.def) - IngestJob.count;
                        if (drugPolicyEntry.allowScheduled && num <= 0)
                        {
                            IngestJob.takeExtraIngestibles = drugPolicyEntry.takeToInventory;
                        }
                    }
                    return DrugAIUtility.IngestAndTakeToInventoryJob(thing, pawn, 1);
                }
            }
            tmpDependencies.Clear();
            return null;
        }

        private bool ShouldSatify(Hediff hediff)
        {
            if (!(hediff is Hediff_Dependency hediff_ChemicalDependency))
            {
                return false;
            }
            return hediff_ChemicalDependency.ShouldSatify;
        }

        private Thing FindIngestibleFor(Pawn pawn, Hediff_Dependency dependency)
        {
            ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (IngestibleValidator(pawn, dependency, innerContainer[i]))
                {
                    return innerContainer[i];
                }
            }
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => DrugValidator(pawn, dependency, x));
            if (thing != null)
            {
                return thing;
            }
            if (pawn.IsColonist && pawn.Map != null)
            {
                foreach (Pawn spawnedColonyAnimal in pawn.Map.mapPawns.SpawnedColonyAnimals)
                {
                    foreach (Thing item in spawnedColonyAnimal.inventory.innerContainer)
                    {
                        if (IngestibleValidator(pawn, dependency, item) && !spawnedColonyAnimal.IsForbidden(pawn) && pawn.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
                        {
                            return item;
                        }
                    }
                }
            }
            return null;
        }

        private bool IngestibleValidator(Pawn pawn, Hediff_Dependency dependency, Thing item)
        {
            if (!item.IngestibleNow) return false;
            if (dependency.chemical != null)
            {
                if (!item.def.IsDrug)
                {
                    return false;
                }
                if (item.Spawned && (!pawn.CanReserve(item) || item.IsForbidden(pawn) || !item.IsSociallyProper(pawn)))
                {
                    return false;
                }
                CompDrug compDrug = item.TryGetComp<CompDrug>();
                if (compDrug == null || compDrug.Props.chemical == null || compDrug.Props.chemical != dependency.chemical)
                {
                    return false;
                }
                if (pawn.drugs != null && !pawn.drugs.CurrentPolicy[item.def].allowedForAddiction && (!pawn.InMentalState || pawn.MentalStateDef.ignoreDrugPolicy))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

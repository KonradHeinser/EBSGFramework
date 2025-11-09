using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobGiver_SatisfyDependency : ThinkNode_JobGiver
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
            if (!ModsConfig.BiotechActive)
                return 0f;
            
            if (Cache?.idgGenes.NullOrEmpty() != false)
                return 0f;

            return pawn.NeedToSatisfyIDG(out _, true) ? 9.25f : 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive) return null;
            if (!pawn.NeedToSatisfyIDG(out var tmpDependencies))
                return null;

            tmpDependencies.SortBy(h => 0f - h.Severity);
            foreach (Hediff_Dependency hediff in tmpDependencies)
            {
                Thing thing = hediff.FindIngestibleFor(pawn);
                if (thing != null)
                {
                    Pawn pawn2 = (thing.ParentHolder as Pawn_InventoryTracker)?.pawn;
                    if (pawn2 != null && pawn2 != pawn)
                    {
                        Job takeJob = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, thing, pawn2);
                        takeJob.count = 1;
                        return takeJob;
                    }
                    Job IngestJob = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                    IngestJob.count = 1;
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
                    return IngestJob;
                }
            }
            return null;
        }
    }
}

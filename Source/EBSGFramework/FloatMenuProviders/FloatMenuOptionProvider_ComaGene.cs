using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class FloatMenuOptionProvider_ComaGene : FloatMenuOptionProvider
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

        protected override bool Drafted => false;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            if (!ModsConfig.BiotechActive)
                return false;

            if (Cache?.ComaNeedsExist() != true)
                return false;

            if (context.FirstSelectedPawn.genes?.GetFirstGeneOfType<Gene_Coma>() == null)
                return false;

            return true;
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            Building_Bed bed = clickedThing as Building_Bed;

            if (bed?.def.building?.bed_humanlike != true)
                return null;

            Pawn pawn = context.FirstSelectedPawn;

            // Always ensures the coma gene closest to empty is used. This shouldn't really be needed, but I can't assume that everyone listens to my warning
            Gene_Coma comaGene = pawn.genes.GetFirstGeneOfType<Gene_Coma>();
            float needLevel = comaGene.ComaNeed.CurLevel;
            foreach (Gene gene in pawn.genes.GenesListForReading)
                if (gene is Gene_Coma coma && coma.ComaNeed.CurLevel < needLevel)
                {
                    comaGene = coma;
                    needLevel = coma.ComaNeed.CurLevel;
                }

            if (!pawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
                return new FloatMenuOption("EBSG_CannotRest".Translate(comaGene.ComaExtension.noun).CapitalizeFirst() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            
            AcceptanceReport acceptanceReport2 = bed.CompAssignableToPawn.CanAssignTo(pawn);
            if (!acceptanceReport2.Accepted)
                return new FloatMenuOption("EBSG_CannotRest".Translate(comaGene.ComaExtension.noun).CapitalizeFirst() + ": " + acceptanceReport2.Reason, null);
            
            if ((!bed.CompAssignableToPawn.HasFreeSlot || !RestUtility.BedOwnerWillShare(bed, pawn, pawn.guest.GuestStatus)) && !bed.IsOwner(pawn))
                return new FloatMenuOption("EBSG_CannotRest".Translate(comaGene.ComaExtension.noun).CapitalizeFirst() + ": " + "AssignedToOtherPawn".Translate(bed).CapitalizeFirst(), null);
            
            if (comaGene.ComaExtension.needBedOutOfSunlight)
                foreach (IntVec3 item25 in bed.OccupiedRect())
                    if (item25.GetRoof(bed.Map) == null)
                        return new FloatMenuOption("EBSG_CannotRest".Translate(comaGene.ComaExtension.noun).CapitalizeFirst() + ": " + "ThingIsSkyExposed".Translate(bed).CapitalizeFirst(), null);

            if (RestUtility.IsValidBedFor(bed, pawn, pawn, true, false, false, pawn.GuestStatus))
                return new FloatMenuOption("EBSG_StartRest".Translate(comaGene.ComaExtension.gerund), delegate
                {
                    Job job25 = JobMaker.MakeJob(comaGene.ComaExtension.relatedJob, bed);
                    job25.forceSleep = true;
                    pawn.jobs.TryTakeOrderedJob(job25, JobTag.Misc);
                });

            return null;
        }
    }
}

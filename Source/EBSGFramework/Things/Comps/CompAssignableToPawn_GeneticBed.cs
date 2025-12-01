using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAssignableToPawn_GeneticBed : CompAssignableToPawn_Bed
    {
        new CompProperties_GeneticBed Props => props as CompProperties_GeneticBed;

        public override string CompInspectStringExtra()
        {
            return null;
        }

        public override bool AssignedAnything(Pawn pawn)
        {
            return pawn.ownership.AssignedDeathrestCasket != null;
        }

        public override void TryAssignPawn(Pawn pawn)
        {
            Building_Bed building_Bed = parent as Building_Bed;
            CompComaGeneBindable comaGeneBindable = parent.TryGetComp<CompComaGeneBindable>();
            if (parent.TryGetComp<CompDeathrestBindable>() != null || comaGeneBindable != null)
            {
                pawn.ownership.ClaimDeathrestCasket(building_Bed);
                building_Bed.NotifyRoomAssignedPawnsChanged();
                if (comaGeneBindable != null)
                    comaGeneBindable.BindTo(pawn);
                return;
            }
            base.TryAssignPawn(pawn);
        }

        public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
        {
            CompComaGeneBindable comaGeneBindable = parent.TryGetComp<CompComaGeneBindable>();
            if (parent.TryGetComp<CompDeathrestBindable>() != null || comaGeneBindable != null)
            {
                if (comaGeneBindable != null)
                    comaGeneBindable.Unbind();
                else
                {
                    Building_Bed ownedBed = pawn.ownership.AssignedDeathrestCasket;
                    pawn.ownership.UnclaimDeathrestCasket();
                    ownedBed?.NotifyRoomAssignedPawnsChanged();
                }
            }
            else base.TryUnassignPawn(pawn, sort, uninstall);
        }

        public override AcceptanceReport CanAssignTo(Pawn pawn)
        {
            CompDeathrestBindable compDeathrestBindable = parent.TryGetComp<CompDeathrestBindable>();
            if (compDeathrestBindable?.BoundPawn != null && compDeathrestBindable.BoundPawn != pawn)
                return "CannotAssignAlreadyBound".Translate(compDeathrestBindable.BoundPawn);

            CompComaGeneBindable compComaRestBindable = parent.TryGetComp<CompComaGeneBindable>();
            if (compComaRestBindable?.BoundPawn != null && compComaRestBindable.BoundPawn != pawn)
                return "CannotAssignAlreadyBound".Translate(compComaRestBindable?.BoundPawn);

            if (!pawn.CheckGeneTrio(Props.anyOfGenes, Props.allOfGenes, Props.noneOfGenes))
                return "EBSG_InvalidGenes".Translate();

            return base.CanAssignTo(pawn);
        }

        protected override void PostPostExposeData()
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit && ModsConfig.BiotechActive && assignedPawns.RemoveAll(x => x.ownership.AssignedDeathrestCasket != parent) > 0)
            {
                Log.Warning(parent.ToStringSafe() + " had pawns assigned that don't have it as an assigned bed. Removing.");
            }
        }
    }
}

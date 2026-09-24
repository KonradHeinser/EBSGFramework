using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class Building_PawnCrate : Building_Crate
    {
        private EBSGExtension extension;

        public EBSGExtension Extension => extension ?? (extension = def.GetModExtension<EBSGExtension>());
        
        private bool setup;

        private Pawn lastDamager; // This is used in case the destroyer doesn't have a job that specifically targets the crate
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!setup)
            {
                var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Extension?.pawnKind ?? EBSGDefOf.EBSG_BlankColonist, 
                    forceGenerateNewPawn: true, forcedXenotype: Extension?.xenotype ?? XenotypeDefOf.Baseliner, 
                    developmentalStages: Extension?.developmentalStage ?? DevelopmentalStage.Adult));
                if (pawn != null)
                {
                    innerContainer.TryAddOrTransfer(pawn);
                    setup = true;
                }
            }
        }

        public override void Open()
        {
            SetPawnFaction();
            base.Open();
            Destroy();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            SetPawnFaction();
            base.Destroy(mode);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (dinfo.Instigator is Pawn p)
                lastDamager = p;
        }

        private void SetPawnFaction()
        {
            if (!setup || !HasAnyContents) return;
            var pawn = MapHeld?.mapPawns?.AllHumanlikeSpawned?.FirstOrDefault(p => p.CurJob?.targetA.Thing == this || p.CurJob?.targetB.Thing == this);
            innerContainer.First().SetFaction((pawn ?? lastDamager)?.Faction ?? Faction.OfPlayer);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref setup, "setup");
        }
    }
}

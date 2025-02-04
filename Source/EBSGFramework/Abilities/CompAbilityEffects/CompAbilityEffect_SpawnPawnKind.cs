using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class CompAbilityEffect_SpawnPawnKind : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnPawnKind Props => (CompProperties_AbilitySpawnPawnKind)props;

        public Pawn Caster => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Map map = Caster.MapHeld;
            if (map == null) return; // Shouldn't be an issue, but better safe than erroring
            IntVec3 pos = target.Cell;

            PawnGenerationRequest request = new PawnGenerationRequest(Props.pawnKind,
                Props.inCreatorFaction ? Caster.Faction : (Find.FactionManager.FirstFactionOfDef(Props.staticFaction) ?? Find.FactionManager.FirstFactionOfDef(Props.pawnKind.defaultFactionType)),
                forceGenerateNewPawn: true, developmentalStages: Props.stage);

            Lord lord = Caster.GetLord();

            int numToSpawn = Props.count.RandomInRange;

            for (int i = 0; i < numToSpawn; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                GenSpawn.Spawn(pawn, pos, map);
                lord?.AddPawn(pawn);

                if (Props.inCreatorFaction && pawn.RaceProps.IsMechanoid && Caster.mechanitor?.CanOverseeSubject(pawn) == true)
                {
                    pawn.GetOverseer()?.relations.TryRemoveDirectRelation(PawnRelationDefOf.Overseer, pawn);
                    Caster.relations?.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
                    Caster.mechanitor.CanOverseeSubject(pawn);
                }

                if (Props.mentalStateOnSpawn != null)
                    pawn.mindState?.mentalStateHandler?.TryStartMentalState(Props.mentalStateOnSpawn);

                if (Props.spawnEffecter != null)
                {
                    Effecter effecter = new Effecter(Props.spawnEffecter);
                    effecter.Trigger(Props.attachEffecterToPawn ? pawn : new TargetInfo(pos, map), TargetInfo.Invalid);
                    effecter.Cleanup();
                }

                if (Props.hediffOnPawns != null)
                    pawn.AddOrAppendHediffs(Props.severity, Props.severity, Props.hediffOnPawns, null, Caster);
            }

            if (Props.effecter != null)
            {
                Effecter effecter = new Effecter(Props.effecter);
                effecter.Trigger(Caster, TargetInfo.Invalid);
                effecter.Cleanup();
            }
        }
    }
}

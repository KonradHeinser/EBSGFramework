using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class HediffComp_SpawnPawnKindOnRemoval : HediffComp
    {
        HediffCompProperties_SpawnPawnKindOnRemoval Props => (HediffCompProperties_SpawnPawnKindOnRemoval)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            Map map = Pawn.MapHeld;
            if (map == null) return; // Ensures there is a map to target, and that the pawn is spawned or inside something
            IntVec3 pos = Pawn.PositionHeld;

            switch (Props.activation)
            {
                case RemovedHediffEffectTrigger.OnDeathOnly:
                    if (!Pawn.Dead)
                        return;
                    break;
                case RemovedHediffEffectTrigger.OnRemovalOnly:
                    if (Pawn.Dead)
                        return;
                    break;
            }

            PawnGenerationRequest request = new PawnGenerationRequest(Props.pawnKind, Props.inCreatorFaction ? Pawn.Faction : null, forceGenerateNewPawn: true,
                    developmentalStages: Props.stage);

            Lord lord = Pawn.GetLord();

            int numToSpawn = Props.count.RandomInRange;

            for (int i = 0; i < numToSpawn; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                GenSpawn.Spawn(pawn, pos, map);
                lord?.AddPawn(pawn);

                if (Props.inCreatorFaction && pawn.RaceProps.IsMechanoid && Pawn.mechanitor?.CanOverseeSubject(pawn) == true)
                {
                    pawn.GetOverseer()?.relations.TryRemoveDirectRelation(PawnRelationDefOf.Overseer, pawn);
                    Pawn.relations?.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
                    Pawn.mechanitor.CanOverseeSubject(pawn);
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
                    pawn.AddOrAppendHediffs(Props.severity, Props.severity, Props.hediffOnPawns, null, Pawn);
            }

            if (Props.effecter != null)
            {
                Effecter effecter = new Effecter(Props.effecter);
                effecter.Trigger(Pawn.Dead ? new TargetInfo(pos, map) : Pawn, TargetInfo.Invalid);
                effecter.Cleanup();
            }
        }
    }
}

using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class HediffComp_SpawnPawnKindOnRemoval : HediffComp
    {
        HediffCompProperties_SpawnPawnKindOnRemoval Props => (HediffCompProperties_SpawnPawnKindOnRemoval)props;

        Thing instigator = null;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            instigator = dinfo?.Instigator;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            Thing source = instigator ?? Pawn;
            Pawn sourcePawn = source as Pawn;

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

            PawnGenerationRequest request = new PawnGenerationRequest(Props.pawnKind, 
                    Props.inCreatorFaction ? source.Faction : null, forceGenerateNewPawn: true,
                    developmentalStages: Props.stage);

            Lord lord = null;
            if (instigator != null)
                if (sourcePawn != null)
                    lord = sourcePawn.GetLord();
            else
                lord = Pawn.GetLord();

            int numToSpawn = Props.count.RandomInRange;

            for (int i = 0; i < numToSpawn; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                GenSpawn.Spawn(pawn, pos, map);
                lord?.AddPawn(pawn);

                if (Props.inCreatorFaction && pawn.RaceProps.IsMechanoid && sourcePawn.mechanitor?.CanOverseeSubject(pawn) == true)
                {
                    pawn.GetOverseer()?.relations.TryRemoveDirectRelation(PawnRelationDefOf.Overseer, pawn);
                    sourcePawn.relations?.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
                    sourcePawn.mechanitor.CanOverseeSubject(pawn);
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
                    pawn.AddOrAppendHediffs(Props.severity, Props.severity, Props.hediffOnPawns, null, sourcePawn);
            }

            if (Props.effecter != null)
            {
                Effecter effecter = new Effecter(Props.effecter);
                effecter.Trigger(Pawn.Dead ? new TargetInfo(pos, map) : Pawn, TargetInfo.Invalid);
                effecter.Cleanup();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref instigator, "instigator");
        }
    }
}

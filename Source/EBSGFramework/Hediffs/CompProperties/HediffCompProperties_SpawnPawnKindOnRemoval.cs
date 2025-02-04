using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SpawnPawnKindOnRemoval : HediffCompProperties
    {
        public RemovedHediffEffectTrigger activation = RemovedHediffEffectTrigger.Always;

        public PawnKindDef pawnKind;

        public IntRange count = new IntRange(1, 1);

        public DevelopmentalStage stage = DevelopmentalStage.Adult;

        public EffecterDef effecter;

        public EffecterDef spawnEffecter;

        public bool attachEffecterToPawn = false;

        public bool inCreatorFaction = false;

        public FactionDef staticFaction;

        public MentalStateDef mentalStateOnSpawn;

        public HediffDef hediffOnPawns;

        public float severity = 1f;

        public FloatRange validSeverity = FloatRange.Zero;

        public HediffCompProperties_SpawnPawnKindOnRemoval()
        {
            compClass = typeof(HediffComp_SpawnPawnKindOnRemoval);
        }
    }
}

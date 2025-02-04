using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilitySpawnPawnKind : CompProperties_AbilityEffect
    {
        public PawnKindDef pawnKind;

        public IntRange count = new IntRange(1, 1);

        public DevelopmentalStage stage = DevelopmentalStage.Adult;

        public EffecterDef effecter;

        public EffecterDef spawnEffecter;

        public bool attachEffecterToPawn = false;

        public bool inCreatorFaction = true;

        public FactionDef staticFaction;

        public MentalStateDef mentalStateOnSpawn;

        public HediffDef hediffOnPawns;

        public float severity = 1f;

        public CompProperties_AbilitySpawnPawnKind()
        {
            compClass = typeof(CompAbilityEffect_SpawnPawnKind);
        }
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SpawnPawnKindOnRemoval : HediffCompProperties
    {
        public RemovedHediffEffectTrigger activation = RemovedHediffEffectTrigger.Always;

        PawnKindDef pawnKind;

        public IntRange count = new IntRange(1, 1);

        public DevelopmentalStage stage = DevelopmentalStage.Adult;

        public HediffCompProperties_SpawnPawnKindOnRemoval()
        {
        }
    }
}

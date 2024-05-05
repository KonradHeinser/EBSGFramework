using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_NearWater : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (def.stages.Count < 2 || p.MapHeld == null) return ThoughtState.Inactive;

            EBSGExtension extension = def.GetModExtension<EBSGExtension>();

            if (extension == null)
            {
                if (EBSGUtilities.CheckNearbyWater(p, 1, out int waterCount)) return GetThoughtState(1);
                return GetThoughtState(0);
            }
            if (!extension.requiredGenes.NullOrEmpty() && !EBSGUtilities.PawnHasAnyOfGenes(p, out var gene, extension.relatedGenes)) return ThoughtState.Inactive;

            if (!EBSGUtilities.CheckNearbyWater(p, 1, out int count, extension.maxWaterDistance)) return GetThoughtState(0);
            if (extension.thresholds.NullOrEmpty())
            {
                if (extension.waterTilesNeeded <= count) return GetThoughtState(1);
                return GetThoughtState(0);
            }
            for (int i = extension.thresholds.Count; i > 0; i--)
            {
                if (count >= extension.thresholds[i - 1]) return ThoughtState.ActiveAtStage(i);
            }
            return GetThoughtState(0);
        }

        public ThoughtState GetThoughtState(int stage)
        {
            if (def.stages[stage].baseMoodEffect != 0) return ThoughtState.ActiveAtStage(stage);
            return ThoughtState.Inactive;
        }
    }
}

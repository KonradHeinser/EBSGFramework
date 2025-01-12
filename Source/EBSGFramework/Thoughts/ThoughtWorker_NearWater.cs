using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_NearWater : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (def.stages.Count < 2 || p.MapHeld == null) return ThoughtState.Inactive;

            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (thoughtExtension != null)
            {
                if (!thoughtExtension.requiredGenes.NullOrEmpty() && !p.PawnHasAnyOfGenes(out var g, thoughtExtension.relatedGenes)) return ThoughtState.Inactive;

                if (!p.CheckNearbyWater(1, out int c, thoughtExtension.maxWaterDistance)) return GetThoughtState(0);
                if (thoughtExtension.thresholds.NullOrEmpty())
                {
                    if (thoughtExtension.waterTilesNeeded <= c) return GetThoughtState(1);
                    return GetThoughtState(0);
                }
                for (int i = thoughtExtension.thresholds.Count; i > 0; i--)
                {
                    if (c >= thoughtExtension.thresholds[i - 1]) return ThoughtState.ActiveAtStage(i);
                }
                return GetThoughtState(0);
            }

            EBSGExtension extension = def.GetModExtension<EBSGExtension>();

            if (extension == null)
            {
                if (p.CheckNearbyWater(1, out int waterCount)) return GetThoughtState(1);
                return GetThoughtState(0);
            }
            if (!extension.requiredGenes.NullOrEmpty() && !p.PawnHasAnyOfGenes(out var gene, extension.relatedGenes)) return ThoughtState.Inactive;

            if (!p.CheckNearbyWater(1, out int count, extension.maxWaterDistance)) return GetThoughtState(0);
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

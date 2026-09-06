using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_DislikedInFaction : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.MapHeld == null) return false;
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            var maxOpinion = thoughtExtension?.maxOpinion ?? 0;
            var count = p.MapHeld?.mapPawns?.FreeHumanlikesSpawnedOfFaction(p.Faction)?.Count(f => (f.relations?.OpinionOf(p) ?? 0) < maxOpinion) ?? 0;
            
            if (count == 0 && thoughtExtension?.curve == null)
                return def.stages.Count > 1 ? ThoughtState.ActiveAtStage(1) : false;

            if (thoughtExtension?.curve != null)
            {
                count = Mathf.FloorToInt(thoughtExtension.curve.Evaluate(count)) - 1;
                return count < 0 ? false : ThoughtState.ActiveAtStage(Mathf.Min(count, def.stages.Count - 1));
            }

            return true;
        }
    }
}
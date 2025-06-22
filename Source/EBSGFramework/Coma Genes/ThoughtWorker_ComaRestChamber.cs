using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_ComaRestChamber : ThoughtWorker
    {
        private static EBSGCache_Component cache;

        public static EBSGCache_Component Cache
        {
            get
            {
                if (cache == null)
                    cache = Current.Game.GetComponent<EBSGCache_Component>();

                if (cache != null && cache.loaded)
                    return cache;
                return null;
            }
        }

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (Cache?.ComaNeedsExist() != true) return ThoughtState.Inactive;
            if (p.story?.traits?.HasTrait(TraitDefOf.Ascetic) == true) return ThoughtState.Inactive;

            if (!p.HasRelatedGene(def.GetModExtension<ComaExtension>().relatedGene)) return ThoughtState.Inactive;
            if (!(p.genes.GetGene(def.GetModExtension<ComaExtension>().relatedGene) is Gene_Coma comaGene) || comaGene.ComaNeed.Comatose) return ThoughtState.Inactive;

            Thing boundBed = comaGene.BoundBed;
            if (boundBed == null) return ThoughtState.Inactive;

            Room room = boundBed.GetRoom();
            if (room == null) return ThoughtState.Inactive;
            int stage;
            // If they are not wild or it is supposed to be outside, set the stage to 0
            if ((!p.IsWildMan() || def.stages[0].baseMoodEffect > 0) && room.PsychologicallyOutdoors) stage = 0;
            else stage = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness)) + 1;
            if (def.stages[stage] != null)
                return ThoughtState.ActiveAtStage(stage);
            return ThoughtState.Inactive;
        }
    }
}

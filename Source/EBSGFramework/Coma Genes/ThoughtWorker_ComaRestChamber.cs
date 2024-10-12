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
            if (Cache?.ComaNeedsExist() != true) return false;
            if (p.story.traits.HasTrait(TraitDefOf.Ascetic)) return false;

            if (!(p.genes.GetGene(def.GetModExtension<ComaExtension>().relatedGene) is Gene_Coma comaGene) || comaGene.ComaNeed.Comatose) return false;

            Thing boundBed = comaGene.BoundBed;
            if (boundBed == null) return false;

            Room room = boundBed.GetRoom();
            if (room == null) return false;
            int stage = -1;
            // If they are not wild or it is supposed to be outside, set the stage to 0
            if ((!p.IsWildMan() || def.stages[0].baseMoodEffect > 0) && room.PsychologicallyOutdoors) stage = 0;
            else stage = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness)) + 1;

            return ThoughtState.ActiveAtStage(stage);
        }
    }
}

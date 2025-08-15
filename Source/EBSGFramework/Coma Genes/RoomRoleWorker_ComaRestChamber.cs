using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class RoomRoleWorker_ComaRestChamber : RoomRoleWorker
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

        public override string PostProcessedLabel(string baseLabel, Room room)
        {
            GeneDef primary = PrimaryComaGene(room, out _);
            if (primary == null)
                return base.PostProcessedLabel(baseLabel, room);

            string staticName = primary.GetModExtension<ComaExtension>().chamberName;

            return staticName != null ? staticName.TranslateOrFormat() : "EBSG_ComaChamber".Translate(primary.LabelCap).ToString();
        }

        public override float GetScore(Room room)
        {
            if (Cache?.ComaNeedsExist() != true)
                return -1f;

            GeneDef primary = PrimaryComaGene(room, out var score);
            if (primary == null) return -1f;
            return score;
        }

        private GeneDef PrimaryComaGene(Room room, out float score)
        {
            Dictionary<GeneDef, float> counter = new Dictionary<GeneDef, float>();
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            if (!containedAndAdjacentThings.NullOrEmpty())
                foreach (Thing thing in containedAndAdjacentThings)
                {
                    CompComaGeneBindable comp = thing.TryGetComp<CompComaGeneBindable>();
                    if (comp != null)
                        foreach (GeneDef gene in comp.Props.relatedGenes)
                        {
                            if (!counter.ContainsKey(gene)) counter.Add(gene, 0f);

                            if (thing.def.IsBed) counter[gene] += 1f;
                            else counter[gene] += 0.5f;
                        }
                }

            score = -1f;

            if (counter.NullOrEmpty())
                return null;

            GeneDef primary = null;
            foreach (GeneDef gene in counter.Keys)
                if (counter[gene] > score)
                {
                    primary = gene;
                    score = counter[gene];
                }
            score *= 100;
            return primary;
        }
    }
}

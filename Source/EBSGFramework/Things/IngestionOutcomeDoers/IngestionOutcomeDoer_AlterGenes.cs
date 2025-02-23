using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class IngestionOutcomeDoer_AlterGenes : IngestionOutcomeDoer
    {
        public List<GeneDef> addedGenes;

        public bool inheritable = true;

        public List<GeneDef> removedGenes;

        public List<RandomXenoGenes> geneSets;

        public bool removeGenesFromOtherLists = true;

        public bool showMessage = true;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            pawn.GainRandomGeneSet(inheritable, removeGenesFromOtherLists, geneSets, addedGenes, removedGenes, showMessage);
        }
    }
}

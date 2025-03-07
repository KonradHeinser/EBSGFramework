using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TerrainCostOverride : HediffCompProperties
    {
        public int universalCostOverride = -1;

        public List<NeedLevel> pawnNeedLevels;

        // Pawn Genes
        public List<GeneDef> pawnHasAnyOfGenes;
        public List<GeneDef> pawnHasAllOfGenes;
        public List<GeneDef> pawnHasNoneOfGenes;

        // Pawn Hediffs
        public List<HediffDef> pawnHasAnyOfHediffs;
        public List<HediffDef> pawnHasAllOfHediffs;
        public List<HediffDef> pawnHasNoneOfHediffs;

        // Pawn Checks
        public List<CapCheck> pawnCapLimiters;
        public List<SkillLevel> pawnSkillLimiters;
        public List<StatCheck> pawnStatLimiters;

        public List<TerrainLinker> terrainSets;

        public HediffCompProperties_TerrainCostOverride()
        {
            compClass = typeof(HediffComp_TerrainCostOverride);
        }
    }
}

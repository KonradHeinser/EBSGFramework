﻿using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class TerrainLinker
    {
        public TerrainDef terrain;

        public int newCost = 1;

        public List<GeneDef> pawnHasAnyOfGenes;
        public List<GeneDef> pawnHasAllOfGenes;
        public List<GeneDef> pawnHasNoneOfGenes;

        // Pawn Hediffs
        public List<HediffDef> pawnHasAnyOfHediffs;
        public List<HediffDef> pawnHasAllOfHediffs;
        public List<HediffDef> pawnHasNoneOfHediffs;

        // Pawn Checks
        public List<CapCheck> pawnCapLimiters;
        public List<SkillCheck> pawnSkillLimiters;
        public List<StatCheck> pawnStatLimiters;
    }
}

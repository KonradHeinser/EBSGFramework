﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class EBSGExtension : DefModExtension
    {
        public SimpleCurve peopleToMoodCurve;
        public GeneDef relatedGene;
        public bool checkNotPresent = false;
        public List<GeneDef> requiredGenesToEquip; // Require all of these on the pawn
        public List<GeneDef> requireOneOfGenesToEquip; // Require any one of these on the pawn
        public List<GeneDef> forbiddenGenesToEquip; // Require none of these are on the pawn
        public List<XenotypeDef> requireOneOfXenotypeToEquip; // Require one of these xenotypes
        public List<XenotypeDef> forbiddenXenotypesToEquip; // Require pawn is not xenotype

        // Curves that can be added to a gene to give pawns an additional age multiplier
        public SimpleCurve maleFertilityAgeFactor;
        public SimpleCurve femaleFertilityAgeFactor;
        public SimpleCurve fertilityAgeFactor;

        // Sets minimum fertility based on age
        public SimpleCurve minFertilityByAgeFactor;
        public SimpleCurve minMaleFertilityByAgeFactor;
        public SimpleCurve minFemaleFertilityByAgeFactor;

        // Sets maximum fertility based on age
        public SimpleCurve maxFertilityByAgeFactor;
        public SimpleCurve maxMaleFertilityByAgeFactor;
        public SimpleCurve maxFemaleFertilityByAgeFactor;

        // Static min fertility
        public float minFertility = 0;
        public float minMaleFertility = 0;
        public float minFemaleFertility = 0;

        // Static max fertility
        public float maxFertility = 999999;
        public float maxMaleFertility = 999999;
        public float maxFemaleFertility = 999999;
    }
}

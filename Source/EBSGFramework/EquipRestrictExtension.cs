using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class EquipRestrictExtension : DefModExtension
    {
        // Attached to things
        public List<GeneDef> requiredGenesToEquip; // Require all of these on the pawn
        public List<GeneDef> requireOneOfGenesToEquip; // Require any one of these on the pawn
        public List<GeneDef> forbiddenGenesToEquip; // Require none of these are on the pawn
        public List<XenotypeDef> requireOneOfXenotypeToEquip; // Require one of these xenotypes
        public List<XenotypeDef> forbiddenXenotypesToEquip; // Require pawn is not xenotype
        public List<HediffDef> requiredHediffsToEquip; // Require all of these on the pawn
        public List<HediffDef> requireOneOfHediffsToEquip; // Require any one of these on the pawn
        public List<HediffDef> forbiddenHediffsToEquip; // Require none of these are on the pawn

        // Attached to genes and xenotypes
    }
}

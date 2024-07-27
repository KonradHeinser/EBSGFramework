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
        public List<ThingDef> limitedToEquipments; // If this is not empty, then the xenotype/carriers of the gene will ONLY be able to equip these things
        public bool noEquipment = false; // Stops all items from being equipped
        public List<ThingDef> limitedToApparels; // If this is not empty, then the xenotype/carriers will ONLY be able to equip apparel from this list
        public bool noApparel = false; // Stops all apparel from being equipped
        public List<ThingDef> limitedToWeapons; // See above, but for weapons
        public bool noWeapons = false; // Stops all weapons from being equipped
        public bool onlyRanged = false; // Won't stop melee attacks from fists and weapon tools, but will stop melee weapons
        public bool onlyMelee = false; // Won't stop ranged abilities from being used
        public List<ThingDef> forbiddenEquipments; // Stops xenotypes/carriers of the gene from equipping anything on the list
    }
}

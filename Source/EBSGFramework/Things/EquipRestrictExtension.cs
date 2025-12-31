using System.Collections.Generic;
using System.Linq;
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
        public List<GeneticTraitData> requireOneOfTraitsToEquip;
        public List<GeneticTraitData> requiredTraitsToEquip;
        public List<GeneticTraitData> forbiddenTraitsToEquip;
        
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
        public List<ApparelLayerDef> restrictedLayers; // Stops any equipment from being placed on this layer
        public List<ThingDef> layerEquipExceptions; // Exceptions to the restrictedLayers tag
        
        // Equipment itself attempting to restrict
        public bool CanEquipGeneCheck(Pawn_GeneTracker tracker)
        {
            // Make sure the pawn even has genes
            if (tracker?.GenesListForReading.NullOrEmpty() != false)
                return requiredGenesToEquip.NullOrEmpty() && requireOneOfGenesToEquip.NullOrEmpty() && requireOneOfXenotypeToEquip.NullOrEmpty();

            // Check the pawn's xenotype
            if (!tracker.CheckXenotype(out bool missing, requireOneOfXenotypeToEquip, forbiddenXenotypesToEquip))
            {
                return false;
            }

            if (!requireOneOfGenesToEquip.NullOrEmpty() && !tracker.TrackerHasAnyOfGenes(out var first, requireOneOfGenesToEquip))
            {
                return false;
            }

            if (!tracker.TrackerHasAllOfGenes(requiredGenesToEquip))
            {
                return false;
            }

            if (tracker.TrackerHasAnyOfGenes(out var firstForbid, forbiddenGenesToEquip))
            {
                return false;
            }
            
            return true;
        }

        public bool CanEquipHediffCheck(HediffSet hediffSet)
        {
            if (!requireOneOfHediffsToEquip.NullOrEmpty() && !hediffSet.SetHasAnyOfHediff(requireOneOfHediffsToEquip, out var first))
            {
                return false;
            }

            if (!hediffSet.SetHasAllOfHediff(requiredHediffsToEquip))
            {
                return false;
            }

            if (hediffSet.SetHasAnyOfHediff(forbiddenHediffsToEquip, out var firstForbid))
            {
                return false;
            }
            
            return true;
        }

        public bool CanEquipTraitCheck(Pawn_StoryTracker tracker)
        {
            if (tracker?.traits?.allTraits.NullOrEmpty() != false)
                return false;

            if (!requireOneOfTraitsToEquip.NullOrEmpty() && !requireOneOfTraitsToEquip.Any(t => tracker.traits.HasTrait(t.def, t.degree)))
            {
                return false;
            }
            
            if (!requiredTraitsToEquip.NullOrEmpty() && !requiredTraitsToEquip.All(t => tracker.traits.HasTrait(t.def, t.degree)))
            {
                return false;
            }

            if (!forbiddenTraitsToEquip.NullOrEmpty() && forbiddenTraitsToEquip.Any(t => tracker.traits.HasTrait(t.def, t.degree)))
            {
                return false;
            }
            
            return true;
        }

        public bool CanEquip(Pawn pawn)
        {
            if (!CanEquipTraitCheck(pawn.story))
            {
                return false;
            }
            
            if (!CanEquipGeneCheck(pawn.genes))
            {
                return false;
            }

            if (!CanEquipHediffCheck(pawn.health?.hediffSet))
            {
                return false;
            }

            return true;
        }

        // Gene or xenotype attempting to restrict
        public bool NeedEquipmentCheck => !limitedToEquipments.NullOrEmpty() || !forbiddenEquipments.NullOrEmpty();
        public bool NeedApparelCheck => !limitedToApparels.NullOrEmpty() || NeedEquipmentCheck;
        public bool NeedWeaponCheck => !limitedToWeapons.NullOrEmpty() || NeedEquipmentCheck;
        
        public bool CanEquipEquipment(Thing thing)
        {
            if (noEquipment)
            {
                return false;
            }

            if (!limitedToEquipments.NullOrEmpty() && !limitedToEquipments.Contains(thing.def))
            {
                return false;
            }

            if (!forbiddenEquipments.NullOrEmpty() && forbiddenEquipments.Contains(thing.def))
            {
                return false;
            }
            
            return true;
        }

        public bool CanEquipWeapon(ThingWithComps weapon)
        {
            if (noWeapons)
                return false;

            if (weapon.def.IsMeleeWeapon && onlyRanged)
                return false;

            if (weapon.def.IsRangedWeapon && onlyMelee)
                return false;

            if (!limitedToWeapons.NullOrEmpty() && !limitedToEquipments.Contains(weapon.def))
                return false;                
            
            return CanEquipEquipment(weapon);
        }

        public bool CanEquipApparel(Apparel apparel)
        {
            if (noApparel)
            {
                return false;
            }
            
            if (!limitedToApparels.NullOrEmpty() && !limitedToApparels.Contains(apparel.def))
            {
                return false;
            }
            
            if (!restrictedLayers.NullOrEmpty() && (layerEquipExceptions.NullOrEmpty() || !layerEquipExceptions.Contains(apparel.def)) && 
                apparel.def.apparel?.layers?.NullOrEmpty() == false && apparel.def.apparel.layers.Any(layer => restrictedLayers.Contains(layer)))
            {
                return false;
            }

            return CanEquipEquipment(apparel);
        }
    }
}

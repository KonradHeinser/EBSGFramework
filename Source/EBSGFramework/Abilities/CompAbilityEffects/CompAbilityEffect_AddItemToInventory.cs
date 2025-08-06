using Verse;
using Verse.Sound;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_AddItemToInventory : CompAbilityEffect
    {
        public new CompProperties_AddItemToInventory Props => (CompProperties_AddItemToInventory)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.targetThing != null && target.TargetIsPawn(out Pawn targetPawn))
            {
                Thing thing = ThingMaker.MakeThing(Props.targetThing, Props.targetStuffing);
                thing.stackCount = Props.targetCount;

                

                if (targetPawn.inventory != null && targetPawn.inventory.innerContainer.CanAcceptAnyOf(thing))
                    targetPawn.inventory.innerContainer.TryAdd(thing, Props.targetCount);
            }

            if (Props.casterThing != null && parent.pawn.inventory != null)
            {
                AddItemToInventory(parent.pawn, Props.casterThing, Props.casterStuffing, Props.casterCount);
            }
        }

        private void AddItemToInventory(Pawn pawn, ThingDef thing, ThingDef stuff, int count)
        {
            if (pawn.inventory == null)
                return;
            Thing item = ThingMaker.MakeThing(thing, stuff);
            item.stackCount = count;

            if (Props.tryEquip && item is ThingWithComps compThing)
            {
                if (compThing.HasComp<CompEquippable>() && CanEquipThing(pawn, compThing))
                {
                    pawn.equipment.AddEquipment(compThing);
                    compThing.def.soundInteract?.PlayOneShot(pawn);
                    return;
                }

                if (compThing is Apparel apparel && CanWearThing(pawn, apparel))
                {
                    pawn.apparel.Wear(apparel);
                    return;
                }
            }
            if (pawn.inventory.innerContainer.CanAcceptAnyOf(item))
                pawn.inventory.innerContainer.TryAdd(item, Props.casterCount);
        }

        private bool CanEquipThing(Pawn pawn, ThingWithComps thing)
        {
            if (pawn.equipment == null)
                return false;

            if (thing.def.equipmentType != EquipmentType.Primary || pawn.equipment.Primary == null)
                return false;

            if (thing.def.IsWeapon && pawn.WorkTagIsDisabled(WorkTags.Violent))
                return false;
            
            if (thing.def.IsRangedWeapon && pawn.WorkTagIsDisabled(WorkTags.Shooting))
                return false;

            if (pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Manipulation) != true)
                return false;

            if (pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(thing, pawn))
                return false;

            if (!EquipmentUtility.CanEquip(thing, pawn))
                return false;

            return true;
        }

        private bool CanWearThing(Pawn pawn, Apparel apparel)
        {
            if (pawn.apparel == null)
                return false;

            if (ApparelUtility.GetApparelReplacedByNewApparel(pawn, apparel) != null)
                return false;

            if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
                return false;

            if (pawn.IsMutant && pawn.mutant.Def.disableApparel)
                return false;

            if (!EquipmentUtility.CanEquip(apparel, pawn))
                return false;

            return true;
        }
    }
}

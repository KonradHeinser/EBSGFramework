using System.Linq;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByEquipped : HediffComp_SetterBase
    {
        private HediffCompProperties_SeverityByEquipped Props => (HediffCompProperties_SeverityByEquipped)props;
        
        private ThingDef weapon;

        private int apparelCount = -1;

        protected override bool DoCheck()
        {
            return (EquipmentCheck() && weapon != Pawn.equipment?.Primary?.def) || 
                   (ApparelCheck() && (Pawn.apparel?.WornApparelCount ?? 0) != apparelCount);
        }

        protected override void SetSeverity()
        {
            base.SetSeverity();

            var flag = false; // Set to true if any valid item is found. If it's false at the end, the comp uses Props.severity instead
            var total = 0f;
            
            switch (Props.check)
            {
                case EquippedCheck.FirstOfEach:
                    flag = FirstWeapon(out total);
                    flag |= FirstApparel(out var amount);
                    total += amount;
                    break;
                case EquippedCheck.ApparelFirst:
                    flag = FirstApparel(out total);
                    if (!flag) flag = FirstWeapon(out total);
                    break;
                case EquippedCheck.EquipmentFirst:
                    flag = FirstWeapon(out total);
                    if (!flag) flag = FirstApparel(out total);
                    break;
                case EquippedCheck.All:
                    flag = FirstWeapon(out total);
                    apparelCount = Pawn.apparel?.WornApparelCount ?? 0;
                    if (apparelCount != 0)
                    {
                        var apparel = Pawn.apparel.WornApparel;
                        var tLinks = Props.apparel?.FindAll(l => apparel.FirstOrDefault(t => t.def == l.thing) != null);
                        if (tLinks?.Any() == true)
                        {
                            flag = true;
                            total += tLinks.Sum(l => l.amount);
                        }

                        var sLinks = Props.apparelTags?.FindAll(l => apparel.FirstOrDefault(t => 
                            t.def.apparel?.tags?.Contains(l.text) == true || t.def.apparel?.defaultOutfitTags?.Contains(l.text) == true) != null);
                        if (sLinks?.Any() == true)
                        {
                            flag = true;
                            total += sLinks.Sum(l => l.num);
                        }
                    }
                    break;
                case EquippedCheck.None:
                default:
                    break;
            }

            parent.Severity = flag ? total : Props.severity;
            
            ticksToNextCheck = 600;
        }

        private bool EquipmentCheck()
        {
            return !Props.equipment.NullOrEmpty() || !Props.equipmentTags.NullOrEmpty();
        }

        private bool FirstWeapon(out float amount)
        {
            amount = 0f;
            if (!EquipmentCheck()) return false;
            weapon = Pawn.equipment?.Primary?.def;
            if (weapon == null) return false; // If they don't have a weapon, obviously they won't have one of the weapons we are looking for
            
            var tLink = Props.equipment?.FirstOrDefault(e => e.thing == weapon);
            if (tLink != null)
            {
                amount = tLink.amount;
                return true;
            }

            var sLink = Props.equipmentTags?.FirstOrDefault(e => weapon.weaponTags?.Contains(e.text) == true);
            if (sLink != null)
            {
                amount = sLink.num;
                return true;
            }
            
            return false;
        }
        
        private bool ApparelCheck()
        {
            return !Props.apparel.NullOrEmpty() || !Props.apparelTags.NullOrEmpty();
        }
        
        private bool FirstApparel(out float amount)
        {
            amount = 0f;
            if (!ApparelCheck()) return false;
            apparelCount = Pawn.apparel?.WornApparelCount ?? 0;
            if (apparelCount == 0) return false; // If they aren't wearing anything, get out of here creep

            var apparel = Pawn.apparel.WornApparel;
            
            var tLink = Props.apparel?.FirstOrDefault(l => apparel.FirstOrDefault(t => t.def == l.thing) != null);
            if (tLink != null)
            {
                amount = tLink.amount;
                return true;
            }

            var sLink = Props.apparelTags?.FirstOrDefault(l => apparel.FirstOrDefault(t => 
                t.def.apparel?.tags?.Contains(l.text) == true || t.def.apparel?.defaultOutfitTags?.Contains(l.text) == true) != null);
            if (sLink != null)
            {
                amount = sLink.num;
                return true;
            }
            
            return false;
        }
    }
}
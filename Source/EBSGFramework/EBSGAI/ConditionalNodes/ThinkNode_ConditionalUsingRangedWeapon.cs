using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalUsingRangedWeapon : ThinkNode_Conditional
    {
        private bool onlyPassIfDraftedOrAttacker = true;

        protected override bool Satisfied(Pawn pawn)
        {
            if (onlyPassIfDraftedOrAttacker && pawn.Faction?.IsPlayer == true && (!pawn.Drafted || !pawn.AutoAttackingColonist())) return false;
            return pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon;
        }
    }
}

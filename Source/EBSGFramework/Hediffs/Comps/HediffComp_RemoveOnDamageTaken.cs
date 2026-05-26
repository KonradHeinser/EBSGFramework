using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemoveOnDamageTaken : HediffComp
    {
        public HediffCompProperties_RemoveOnDamageTaken Props => (HediffCompProperties_RemoveOnDamageTaken)props;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (!Props.amount.ValidValue(dinfo.Amount))
                return;

            if (!Props.damageDefs.NullOrEmpty() &&
                Props.damageDefs.Contains(dinfo.Def) != (Props.checkType == CheckType.Required))
                return;
            
            parent.pawn.health.RemoveHediff(parent);
        }
    }
}
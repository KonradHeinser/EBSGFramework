using System.Text;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemoveWhenDamaged : HediffComp
    {
        private HediffCompProperties_RemoveWhenDamaged Props => props as HediffCompProperties_RemoveWhenDamaged;

        private float remaining = -1f;
        
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            remaining = Props.amount;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if (dinfo.Def.ValidDamage(Props.damageDefs, Props.checkType) && Props.validAmounts.ValidValue(totalDamageDealt))
            {
                remaining -= totalDamageDealt;
                if (remaining <= 0)
                    Pawn.health.RemoveHediff(parent);
            }
        }
        
        public override string CompDebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompDebugString());
            stringBuilder.Append($"Remaining: {remaining}");
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref remaining, "remaining", -1f);
        }
    }
}
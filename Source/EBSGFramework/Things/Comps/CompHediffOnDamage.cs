using Verse;

namespace EBSGFramework
{
    public class CompHediffOnDamage : ThingComp
    {
        private CompProperties_HediffOnDamage Props => props as CompProperties_HediffOnDamage;

        private Pawn Pawn => parent as Pawn;

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            if (Pawn == null) return;

            if ((dinfo.Def.isRanged && !Props.triggeredByRangedDamage) || (dinfo.Def.isExplosive && !Props.triggeredByExplosions)
                || (!dinfo.Def.isRanged && !dinfo.Def.isExplosive && !Props.triggeredByMeleeDamage))
                return;

            if (!Props.whitelistedDamageDefs.NullOrEmpty() && !Props.whitelistedDamageDefs.Contains(dinfo.Def))
                return;

            if (!Props.blacklistedDamageDefs.NullOrEmpty() && Props.blacklistedDamageDefs.Contains(dinfo.Def))
                return;

            Hediff hediff = null;
            bool flag = false;

            if (Props.applyToBodypart)
                flag = Pawn.HasHediff(Props.givenHediff, dinfo.HitPart, out hediff);
            else
                flag = Pawn.HasHediff(Props.givenHediff, out hediff);

            if (flag)
            {
                if (Props.severityPerDamage != null)
                    hediff.Severity += (float)Props.severityPerDamage * dinfo.Amount;
            }
            else
            {
                hediff = Pawn.health.AddHediff(Props.givenHediff, Props.applyToBodypart ? dinfo.HitPart : null);

                if (Props.severityPerDamage != null)
                    hediff.Severity = (float)Props.severityPerDamage * dinfo.Amount;
            }
        }
    }
}

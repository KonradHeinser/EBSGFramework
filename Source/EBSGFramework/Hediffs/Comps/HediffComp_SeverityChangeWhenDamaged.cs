using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityChangeWhenDamaged : HediffComp
    {
        public HediffCompProperties_SeverityChangeWhenDamaged Props => (HediffCompProperties_SeverityChangeWhenDamaged)props;

        public int cooldownTicks = 0;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (cooldownTicks > 0)
                cooldownTicks -= delta;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (cooldownTicks <= 0 && Props.validSeverity.ValidValue(parent.Severity) &&
                (Props.forbiddenDamageDefs.NullOrEmpty() || dinfo.Def == null || !Props.forbiddenDamageDefs.Contains(dinfo.Def)) &&
                (Props.validDamageDefs.NullOrEmpty() || (dinfo.Def != null && Props.validDamageDefs.Contains(dinfo.Def))))
            {
                parent.Severity += Props.severityChange;
                parent.Severity += totalDamageDealt * Props.severityChangeFactor;

                if (Props.cooldownTicks > 0)
                    cooldownTicks = Props.cooldownTicks;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref cooldownTicks, "EBSG_cooldownTicksChangeWhenHit", 0);
        }
    }
}

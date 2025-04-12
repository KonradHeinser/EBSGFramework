using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityChangeWhenDamaged : HediffComp
    {
        public HediffCompProperties_SeverityChangeWhenDamaged Props => (HediffCompProperties_SeverityChangeWhenDamaged)props;

        public int cooldownTicks = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (cooldownTicks > 0) cooldownTicks--;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (cooldownTicks <= 0 && parent.Severity >= Props.minSeverity && parent.Severity < Props.maxSeverity &&
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

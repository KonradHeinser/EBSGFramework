using Verse;

namespace EBSGFramework
{
    public class HediffComp_AlterXenotype : HediffComp
    {
        public HediffCompProperties_AlterXenotype Props => (HediffCompProperties_AlterXenotype)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            CheckAlteration();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            CheckAlteration();
        }

        private void CheckAlteration()
        {
            if (Props.severities.Includes(parent.Severity))
            {
                EBSGUtilities.AlterXenotype(Pawn, Props.xenotypes, Props.filth, Props.filthCount, Props.setXenotype, Props.sendMessage);
                Pawn.health.RemoveHediff(parent);
            }
            else if (Pawn.genes == null)
                Pawn.health.RemoveHediff(parent);
        }
    }
}

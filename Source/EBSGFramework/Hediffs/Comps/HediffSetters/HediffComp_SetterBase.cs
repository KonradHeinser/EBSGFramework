using Verse;

namespace EBSGFramework
{
    public class HediffComp_SetterBase : HediffComp
    {
        protected int ticksToNextCheck;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            SetSeverity();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (ticksToNextCheck <= 0 || DoCheck())
                SetSeverity();
            else
                ticksToNextCheck -= delta;
        }

        protected virtual bool DoCheck()
        {
            return true;
        }

        protected virtual void SetSeverity()
        {

        }
    }
}

using Verse;

namespace EBSGFramework
{
    public class HediffComp_SetterBase : HediffComp
    {
        protected int ticksToNextCheck;

        protected virtual bool MustBeSpawned => false;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            SetSeverity();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (!MustBeSpawned || Pawn.Spawned)
            {
                if (ticksToNextCheck <= 0 || DoCheck())
                    SetSeverity();
                else
                    ticksToNextCheck -= delta;
            }
        }

        protected virtual bool DoCheck()
        {
            return false;
        }

        protected virtual void SetSeverity()
        {

        }
    }
}

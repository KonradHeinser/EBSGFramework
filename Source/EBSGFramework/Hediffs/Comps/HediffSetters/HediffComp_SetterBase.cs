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

            if (!MustBeSpawned || Pawn.Spawned)
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

        protected virtual bool DoCheck() // Overrides the normal ticks check. Should only use lighter checks
        {
            return false;
        }

        protected virtual void SetSeverity()
        {
            // Take effect on the hediff severity and set a new ticksToNextCheck
        }
    }
}

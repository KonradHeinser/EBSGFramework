using Verse;

namespace EBSGFramework
{
    public class HediffComp_SetterBase : HediffComp
    {
        // This is keeping track of the time left until the next check, and must be set within SetSeverity or things go south fast
        // Higher numbers will reduce the impact of the comp, but may result in confused players if there's too much of a delay without any DoCheck overrides
        protected int ticksToNextCheck;

        // Used to make it so checks won't occur if the pawn isn't spawned. Use for situations where the map or the area around the pawn is checked
        protected virtual bool MustBeSpawned => false; 

        // Sets the initial severity. If additional things should happen post-add, make sure you remember the base.CompPostPostAdd
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (!MustBeSpawned || Pawn.Spawned)
                SetSeverity();
        }

        // This will tick down ticksToNextCheck, and if ticks reach 0 or DoCheck overrides the ticks check, it will start the severity setting process
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

        // Overrides the normal ticks check. Should only use lighter checks like comparing the size of a list to the previous count (See severity by genes for example)
        protected virtual bool DoCheck() 
        {
            return false;
        }

        protected virtual void SetSeverity()
        {
            // This should always be added to children, and is responsible for setting the severity and a new ticksToNextCheck
        }
    }
}

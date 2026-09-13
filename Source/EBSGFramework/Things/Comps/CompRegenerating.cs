using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompRegenerating : ThingComp
    {
        public CompProperties_Regenerating Props => (CompProperties_Regenerating)props;

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (parent.IsHashIntervalTick(Props.regenerationInterval, delta) && (parent.HitPoints < parent.MaxHitPoints || Props.regenerationAmount < 0))
            {
                if (Props.regenerationAmount > 0)
                    parent.HitPoints += Props.regenerationAmount;
                else
                    parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, Math.Abs(Props.regenerationAmount)));
            }
        }
    }
}

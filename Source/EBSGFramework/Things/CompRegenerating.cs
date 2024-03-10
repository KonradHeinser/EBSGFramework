using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompRegenerating : ThingComp
    {
        public CompProperties_Regenerating Props => (CompProperties_Regenerating)props;

        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(Props.regenerationInterval) && parent.HitPoints < parent.MaxHitPoints)
            {
                parent.HitPoints += Props.regenerationAmount;
            }
        }
    }
}

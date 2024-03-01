using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodeOnDeath : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodeOnDeath Props => (HediffCompProperties_ExplodeOnDeath)props;

        public override void Notify_PawnDied()
        {
            if (parent.Severity >= Props.minSeverity && parent.Severity < Props.maxSeverity)
                DoExplosion(parent.pawn.Corpse.Position);
        }
    }
}

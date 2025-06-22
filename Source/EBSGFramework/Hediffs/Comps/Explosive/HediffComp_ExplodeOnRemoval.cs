using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodeOnRemoval : HediffComp
    {
        public new HediffCompProperties_ExplodeOnRemoval Props => (HediffCompProperties_ExplodeOnRemoval)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (!Pawn.Dead || Props.allowDead)
                Props.explosion.DoExplosion(Pawn, Pawn.PositionHeld, Pawn.MapHeld, parent.Severity);
        }
    }
}

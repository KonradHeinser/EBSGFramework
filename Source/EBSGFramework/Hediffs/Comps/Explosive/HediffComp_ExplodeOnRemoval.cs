namespace EBSGFramework
{
    public class HediffComp_ExplodeOnRemoval : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodeOnRemoval Props => (HediffCompProperties_ExplodeOnRemoval)props;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Pawn.Dead)
            {
                if (Props.allowDead)
                    DoExplosion(Pawn.Corpse.Position);
            }
            else
                DoExplosion(Pawn.Position);
        }
    }
}

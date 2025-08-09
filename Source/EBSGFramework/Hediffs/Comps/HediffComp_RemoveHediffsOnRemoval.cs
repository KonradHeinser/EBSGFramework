using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemoveHediffsOnRemoval : HediffComp
    {
        public HediffCompProperties_RemoveHediffsOnRemoval Props => props as HediffCompProperties_RemoveHediffsOnRemoval;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Pawn.RemoveHediffsFromParts(Props.hediffs);
        }
    }
}

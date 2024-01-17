using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class Gene_HemoHediffAdder : Gene_HemogenDrain
    {
        public override void PostAdd()
        {
            base.PostAdd();
            HediffAdder.HediffAdding(pawn, this);
        }

        public override void PostRemove()
        {
            base.PostRemove();
            HediffAdder.HediffRemoving(pawn, this);
        }
    }
}

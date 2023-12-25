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
    }
}

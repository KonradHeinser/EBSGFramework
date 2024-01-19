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
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension != null) SpawnAgeLimiter.LimitAge(pawn, extension.expectedAges, extension.ageRange, extension.sameBioAndChrono);
        }

        public override void PostRemove()
        {
            base.PostRemove();
            HediffAdder.HediffRemoving(pawn, this);
        }
    }
}

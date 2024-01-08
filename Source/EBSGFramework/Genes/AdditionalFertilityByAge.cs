using Verse;

namespace EBSGFramework
{
    public class AdditionalFertilityByAge : Gene
    {
        // This class was only made to make it require less work to find out if it's worth going everything in depth.
        public override void PostAdd()
        {
            base.PostAdd();
            HediffAdder.HediffAdding(pawn, this);
        }
    }
}

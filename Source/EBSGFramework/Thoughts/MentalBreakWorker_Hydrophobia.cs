using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class MentalBreakWorker_Hydrophobia : MentalBreakWorker
    {
        public override bool BreakCanOccur(Pawn pawn)
        {
            if (!pawn.Spawned || !base.BreakCanOccur(pawn)) return false;

            EBSGExtension extension = def.GetModExtension<EBSGExtension>();

            if (extension == null)
                return pawn.CheckNearbyWater(1, out int waterCount);

            return pawn.CheckNearbyWater(1, out int count, extension.maxWaterDistance);
        }
    }
}
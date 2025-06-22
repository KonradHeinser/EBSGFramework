using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class MentalBreakWorker_Hydrophobia : MentalBreakWorker
    {
        public override bool BreakCanOccur(Pawn pawn)
        {
            if (!pawn.Spawned || !base.BreakCanOccur(pawn)) return false;

            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (thoughtExtension != null)
                return pawn.CheckNearbyWater(1, out _, thoughtExtension.maxWaterDistance);


            return false;
        }
    }
}
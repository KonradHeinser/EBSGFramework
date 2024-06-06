using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace EBSGFramework
{
    public class JobDriver_Gatherer : JobDriver
    {
        private const int gatherTime = 300; // Time spent standing on a specific cell to "gather" from it
        private int progress = 0; // Ticks already spent
        private int progressNeeded = 1500; // Ticks required
        public Thing GathererCenter => TargetA.Thing;
        public IntVec3 GathererDestination => TargetB.Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            return null;
        }
    }
}

using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class MentalState_PanicFleeWater : MentalState
    {
        private int lastWaterSeenTick = -1;

        protected override bool CanEndBeforeMaxDurationNow => false;

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();

            if (pawn.IsHashIntervalTick(30))
                if (lastWaterSeenTick < 0 || NearbyWater())
                    lastWaterSeenTick = Find.TickManager.TicksGame;
                else if (lastWaterSeenTick >= 0 && Find.TickManager.TicksGame >= lastWaterSeenTick + def.minTicksBeforeRecovery)
                    RecoverFromState();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastWaterSeenTick, "lastWaterSeenTick", -1);
        }

        private bool NearbyWater()
        {
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();

            if (extension == null)
                return pawn.CheckNearbyWater(1, out int waterCount);

            return pawn.CheckNearbyWater(1, out int count, extension.maxWaterDistance);
        }
    }
}
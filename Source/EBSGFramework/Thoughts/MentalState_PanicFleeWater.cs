﻿using RimWorld;
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

        public override void MentalStateTick(int delta)
        {
            base.MentalStateTick(delta);
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
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (thoughtExtension != null)
                return pawn.CheckNearbyWater(1, out _, thoughtExtension.maxWaterDistance);

            EBSGExtension extension = def.GetModExtension<EBSGExtension>();

            if (extension == null)
                return pawn.CheckNearbyWater(1, out _);

            return pawn.CheckNearbyWater(1, out _, extension.maxWaterDistance);
        }
    }
}
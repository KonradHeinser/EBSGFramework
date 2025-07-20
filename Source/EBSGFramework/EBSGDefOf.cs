using RimWorld;
using Verse;

namespace EBSGFramework
{
    [DefOf]
    public static class EBSGDefOf
    {
        [MayRequireBiotech]
        public static StatDef EBSG_PawnGestationSpeed;
        public static StatDef EBSG_BloodlossRecoveryBonus;
        public static DamageDef EBSG_GeneticDamage;

        [MayRequireBiotech]
        public static JobDef DRG_Consume;
        [MayRequireBiotech]
        public static JobDef DRG_Deliver;
        [MayRequireBiotech]
        public static JobDef DRG_FeedPatient;
        [MayRequireBiotech]
        public static ConceptDef EBSG_SpecialComa;

        public static PawnCapacityDef Metabolism;

        public static JobDef EBSG_NeedCharge;
        public static JobDef EBSG_GathererJob;
        public static JobDef EBSG_EnterSleepCasket;
        public static JobDef EBSG_ReloadAbility;

        public static ThingDef EBSG_PawnLeaving;
        public static WorldObjectDef EBSG_PawnFlying;

        public static EBSGRecorder EBSG_Recorder;

        // Need Stuff
        public static StatDef EBSG_HungerRateFactor;
        [MayRequireBiotech]
        public static StatDef EBSG_DeathrestEfficiency;
        [MayRequireBiotech]
        public static StatDef EBSG_DeathrestRiseRate;
        [MayRequireBiotech]
        public static StatDef EBSG_DeathrestFallRate;
        [MayRequireBiotech]
        public static StatDef EBSG_KillThirstRate;
        [MayRequireBiotech]
        public static NeedDef KillThirst;

        public static StatDef EBSG_ComfortRiseRate;
        public static StatDef EBSG_ComfortFallRate;
        public static StatDef EBSG_BeautyRiseRate;
        public static StatDef EBSG_BeautyFallRate;
        public static StatDef EBSG_MoodRiseRate;
        public static StatDef EBSG_MoodFallRate;
        public static StatDef EBSG_JoyRiseRate;
        public static StatDef EBSG_IndoorsRiseRate;
        public static StatDef EBSG_IndoorsFallRate;
        public static StatDef EBSG_OutdoorsRiseRate;
        public static StatDef EBSG_OutdoorsFallRate;

        // More StatDef stuff

        public static StatDef EBSG_SkillLossRate;
        [MayRequireBiotech]
        public static StatDef EBSG_GrowthPointRate;
        public static StatDef EBSG_OutgoingDamageFactor;
        public static StatDef EBSG_PsyfocusFallRateFactor;
        public static StatDef EBSG_PsyfocusFallRateOffset;
        [MayRequireBiotech]
        public static StatDef EBSG_HemogenMaxOffset;
        [MayRequireBiotech]
        public static StatDef EBSG_HemogenMaxFactor;
    }
}

using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    [DefOf]
    public static class EBSGDefOf
    {
        public static StatDef EBSG_PawnGestationSpeed;
        public static StatDef EBSG_BloodlossRecoveryBonus;
        public static StatDef EBSG_Healthiness;
        public static DamageDef EBSG_GeneticDamage;

        public static PawnCapacityDef Metabolism;

        public static JobDef EBSG_NeedCharge;

        // Need Stuff
        public static StatDef EBSG_DeathrestEfficiency;
        public static StatDef EBSG_DeathrestFallRate;
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

        public static StatDef EBSG_GrowthPointRate;
        public static StatDef EBSG_OutgoingDamageFactor;
        public static StatDef EBSG_PsyfocusFallRateFactor;
        public static StatDef EBSG_PsyfocusFallRateOffset;
        public static StatDef EBSG_HemogenMaxOffset;
        public static StatDef EBSG_HemogenMaxFactor;
    }
}

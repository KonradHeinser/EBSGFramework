using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class SpawnAgeLimiter : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension != null) LimitAge(pawn, extension.expectedAges, extension.ageRange, extension.sameBioAndChrono, extension.chronicAgeRemoval);
        }
        
        public static void LimitAge(Pawn pawn, FloatRange expectedAges, FloatRange ageRange, bool sameBioAndChrono = false, bool removeChronic = true)
        {
            if (ageRange.max > 0)
            {
                float currentBioAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                float currentChronoAge = pawn.ageTracker.AgeChronologicalYearsFloat;
                if (!expectedAges.Includes(currentBioAge))
                {
                    currentBioAge = ageRange.RandomInRange;
                    pawn.ageTracker.AgeBiologicalTicks = (long)(currentBioAge * 3600000f);
                }
                if (sameBioAndChrono && currentBioAge != currentChronoAge)
                {
                    pawn.ageTracker.AgeChronologicalTicks = (long)(currentBioAge * 3600000f);
                }
                if (removeChronic) EBSGUtilities.RemoveChronicHediffs(pawn);
            }
        }
    }
}

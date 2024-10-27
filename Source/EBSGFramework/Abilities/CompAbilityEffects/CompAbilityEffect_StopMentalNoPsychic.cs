using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompAbilityEffect_StopMentalNoPsychic : CompAbilityEffect
    {
        public new CompProperties_AbilityStopMentalNoPsychic Props => (CompProperties_AbilityStopMentalNoPsychic)props;

        public override bool HideTargetPawnTooltip => true;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Rand.Chance(SuccessChance(target, out var intensity)) && intensity != MentalBreakIntensity.None)
            {
                Pawn pawn = target.Pawn;
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.CatatonicBreakdown);
                if (firstHediffOfDef != null)
                    pawn.health.RemoveHediff(firstHediffOfDef);
                pawn?.MentalState?.RecoverFromState();
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!Props.mustHaveMentalState) return true;
            if (SuccessChance(target, out var intensity) > 0)
                return true;

            Pawn pawn = target.Pawn;
            if (throwMessages && pawn != null && intensity != MentalBreakIntensity.None)
                Messages.Message("AbilityDoesntWorkOnMentalState".Translate(parent.def.label, pawn.MentalStateDef.label), pawn, MessageTypeDefOf.RejectInput, false);

            return false;
        }

        private float SuccessChance(LocalTargetInfo target, out MentalBreakIntensity intensity)
        {
            Pawn pawn = target.Pawn;
            intensity = MentalBreakIntensity.None;
            if (pawn != null)
            {
                MentalStateDef mentalStateDef = pawn.MentalStateDef;

                if (pawn.health.hediffSet.HasHediff(HediffDefOf.CatatonicBreakdown))
                {
                    if (!Props.canFixCatatonic) return 0f;
                    intensity = MentalBreakIntensity.Extreme;
                    return Props.extremeChance;
                }

                if (!Props.limitedTo.NullOrEmpty() && (mentalStateDef == null || !Props.limitedTo.Contains(mentalStateDef))) return 0f;

                if (mentalStateDef != null)
                {
                    if (!Props.exceptions.NullOrEmpty() && Props.exceptions.Contains(mentalStateDef)) return 0f;
                    List<MentalBreakDef> allDefsListForReading = DefDatabase<MentalBreakDef>.AllDefsListForReading;
                    for (int i = 0; i < allDefsListForReading.Count; i++)
                        if (allDefsListForReading[i].mentalState == mentalStateDef)
                        {
                            intensity = allDefsListForReading[i].intensity;
                            break;
                        }
                }

                switch (intensity)
                {
                    case MentalBreakIntensity.Minor:
                        return Props.minorChance;
                    case MentalBreakIntensity.Major:
                        return Props.majorChance;
                    case MentalBreakIntensity.Extreme:
                        return Props.extremeChance;
                    default:
                        return 0f;
                }
            }
            return 0f;
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Pawn != null && Valid(target))
            {
                float successChance = SuccessChance(target, out _);
                if (successChance > 0)
                    return "EBSG_StopBreak".Translate(successChance * 100);
            }
            return null;
        }
    }
}

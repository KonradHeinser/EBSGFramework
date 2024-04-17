using RimWorld;
using Verse;
using UnityEngine;

namespace EBSGFramework
{
    public class HediffComp_HemogenOnKill : HediffComp
    {
        public HediffCompProperties_HemogenOnKill Props => (HediffCompProperties_HemogenOnKill)props;

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            if (!victim.health.CanBleed || Pawn.genes == null || parent.Severity < Props.minSeverity || parent.Severity > Props.maxSeverity
                || (Props.maxDistance > 0 && Pawn.Position.DistanceTo(victim.Position) > Props.maxDistance) || Props.hemogenEfficiency <= 0) return;

            if (!Props.forbiddenTargetGenes.NullOrEmpty() && EBSGUtilities.HasAnyOfRelatedGene(victim, Props.forbiddenTargetGenes)) return;

            if (victim.RaceProps.Humanlike)
            {
                if (!Props.allowHumanoids) return;
            }
            else if (victim.RaceProps.Dryad)
            {
                if (!Props.allowDryads) return;
            }
            else if (victim.RaceProps.Insect)
            {
                if (!Props.allowInsects) return;
            }
            else if (victim.RaceProps.Animal)
            {
                if (!Props.allowAnimals) return;
            }
            else if (ModsConfig.AnomalyActive && victim.RaceProps.IsAnomalyEntity)
            {
                if (!Props.allowEntities) return;
            }

            Gene_Hemogen hemogen = Pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
            if (hemogen == null) return;

            if (Props.staticHemogenGain != 0f)
            {
                hemogen.Value += Props.staticHemogenGain;
            }
            else
            {
                if (victim.health == null) return;

                // maxGain represents the maximum amount the pawn has a reason to store, while maxToTake represents the hard cap caused by the properties or the maxGain. maxToTake is the bloodloss added
                float maxGain = (hemogen.Max - hemogen.Value) / Props.hemogenEfficiency;
                float maxToTake = Props.maxGainableHemogen / Props.hemogenEfficiency;
                if (maxToTake > 1) maxToTake = 1; // Can never take more than 100% of the victim's blood for obvious reasons

                if (maxToTake > maxGain) maxToTake = maxGain;
                if (maxToTake <= 0) return;

                if (!EBSGUtilities.HasHediff(victim, HediffDefOf.BloodLoss))
                {
                    Hediff bloodloss = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, victim);
                    bloodloss.Severity = maxToTake;
                    victim.health.AddHediff(bloodloss);
                    hemogen.Value += maxToTake * Props.hemogenEfficiency;
                }
                else
                {
                    victim.health.hediffSet.TryGetHediff(HediffDefOf.BloodLoss, out Hediff bloodloss);
                    if (bloodloss.Severity == 1) return;

                    // If maxToTake is trying to take more blood than there is, set the severity to 1 and use the old difference as the hemogen increase base
                    if (maxToTake > 1 - bloodloss.Severity)
                    {
                        maxToTake = 1 - bloodloss.Severity;
                        bloodloss.Severity = 1;
                        hemogen.Value += maxToTake * Props.hemogenEfficiency;
                    }
                    else
                    {
                        bloodloss.Severity += maxToTake;
                        hemogen.Value += maxToTake * Props.hemogenEfficiency;
                    }
                }
            }
        }
    }
}

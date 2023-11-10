using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_RemoveMultipleHediffs : CompAbilityEffect
    {
        public new CompProperties_AbilityRemoveMultipleHediffs Props => (CompProperties_AbilityRemoveMultipleHediffs)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Props.applyToBoth)
            {
                RemoveMultipleHediffs(parent.pawn);
                if (target.Pawn != null)
                {
                    RemoveMultipleHediffs(target.Pawn);
                }
            }
            else if (Props.applyToSelf)
            {
                RemoveMultipleHediffs(parent.pawn);
            }
            else if (target.Pawn != null)
            {
                RemoveMultipleHediffs(target.Pawn);
            }
        }

        private void RemoveMultipleHediffs(Pawn pawn)
        {
            for (var i = 0; i < Props.recursiveChecks; i++)
            {
                bool recursiveWorthIt = false; // This will determine if the loop should end after the list is checked. 
                foreach (var hediff in Props.hediffs)
                {
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
                    if (firstHediffOfDef != null)
                    { 
                        recursiveWorthIt = true; // A match means that there may be child that pops up due to a hediff removal, or that other hediffs may be removed if onlyFirst is true
                        if (Props.onlyFirstMatch == true) // If only one should be removed, then remove this one and get out of here
                        {
                            pawn.health.RemoveHediff(firstHediffOfDef);
                        }
                        else
                        {
                            while (firstHediffOfDef != null) // Otherwise, start going through the hediff list over and over make sure all are removed
                            {
                                pawn.health.RemoveHediff(firstHediffOfDef);
                                firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
                            }
                        }
                    }
                }

                if (recursiveWorthIt == false) // Stop next loop from starting if none of the listed hediffs were found
                {
                    break;
                }
            }
        }
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityBloodDrain : CompProperties_AbilityBloodfeederBite
    {
        // Inherited Customizable Variables

        //      Required
        // float hemogenGain
        // float resistanceGain
        // IntRange bloodFilthToSpawnRange

        //      Optional
        // ThoughtDef thoughtDefToGiveTarget
        // ThoughtDef opinionThoughtDefToGiveTarget
        // float nutritionGain = 0.1f
        // float targetBloodLoss = 0.4499f

        public HediffDef replacementHediff; // Applies this hediff to neck instead of bloodfeeder bite. This can be used to add a hediff to the target without using the addhediff comp. Is optional.
        public HediffDef hediffToSelf;      // Applies this hediff to caster. Is optional.
        public bool damageSelf = false;     // When false, aoe's don't impact this pawn.
                                            
        // Note to self, remember to make an AddMultipleHediffs(hediffsToTarget, hediffsToSelf, hediffsToBoth). Also, remember to delete this note when you do.

        public CompProperties_AbilityBloodDrain()
        {
            compClass = typeof(CompAbilityEffect_BloodDrain);
        }
    }
}

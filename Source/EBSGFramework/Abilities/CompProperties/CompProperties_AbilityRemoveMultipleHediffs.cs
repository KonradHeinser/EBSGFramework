using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompProperties_AbilityRemoveMultipleHediffs : CompProperties_AbilityEffect
    {
        public List<HediffDef> hediffs = new List<HediffDef>();
        public bool onlyFirstMatch = false; // If true, the ability will consider the job done the moment it finds any match. Otherwise, it will get rid of everything on the list in one check
        public int recursiveChecks = 1; // Determines the maximum number of times this should run. If onlyFirst is false, this should be at most 2 or 3

        public bool applyToBoth = false; // If this is true, then the abilty will remove the hediffs from both the target and the caster
        public bool applyToSelf = false; // If this is true while above is false, then the abilty will remove hediffs from the caster instead of the target
                                         // If both are false, the ability will only impact the target, assuming it is a pawn

        // While onlyFirstMatch is false, recursiveChecks acts as a way of ensuring no new hediffs were made due to removing others from the list. 
            // It usually stops after the second check in these situations because it finds nothing, but may do further checks in special situations, like certain part restorations.
        // If onlyFirstMatch is true then recursive checks acts as a remove up to x number of hediffs

        public CompProperties_AbilityRemoveMultipleHediffs()
        {
            compClass = typeof(CompAbilityEffect_RemoveMultipleHediffs);
        }
    }
}

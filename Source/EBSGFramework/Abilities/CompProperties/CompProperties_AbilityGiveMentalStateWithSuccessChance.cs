using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityGiveMentalStateWithSuccessChance : CompProperties_AbilityGiveMentalState
    {
        public SuccessChance successChance;

        public CompProperties_AbilityGiveMentalStateWithSuccessChance()
        {
            compClass = typeof(CompAbilityEffect_GiveMentalStateWithSuccessChance);
        }

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;
            if (successChance == null)
                yield return "successChance is null. If this ability is not intended to have successChance, use the vanilla CompProperties_AbilityGiveMentalState instead.";
        }
    }
}
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse;

namespace EBSGFramework
{
    public class JobGiver_ReloadAbility : ThinkNode_JobGiver
    {
        private static EBSGCache_Component cache;

        public static EBSGCache_Component Cache
        {
            get
            {
                if (cache == null)
                    cache = Current.Game.GetComponent<EBSGCache_Component>();

                if (cache != null && cache.loaded)
                    return cache;
                return null;
            }
        }

        public override float GetPriority(Pawn pawn)
        {
            return Cache?.abilityFuel.NullOrEmpty() == false ? 5.9f : 0;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.abilities == null)
                return null;

            if (EBSGUtilities.PawnHasAnyOfAbilities(pawn, Cache.reloadableAbilities, out var abilities))
                foreach (Ability ability in abilities)
                {
                    var reloadComp = ability.CompOfType<CompAbilityEffect_Reloadable>();

                    if (reloadComp.ChargesNeeded > 0)
                    {
                        List<Thing> validAmmo = RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, pawn.Position,
                            new IntRange(reloadComp.Props.ammoPerCharge, reloadComp.ChargesNeeded * reloadComp.Props.ammoPerCharge),
                            (obj) => obj.def == reloadComp.Props.ammoDef && obj.stackCount >= reloadComp.Props.ammoPerCharge);

                        if (!validAmmo.NullOrEmpty())
                        {
                            validAmmo.SortBy((arg) => arg.Position.DistanceTo(pawn.Position));
                            Thing thing = validAmmo[0];
                            Job job = JobMaker.MakeJob(EBSGDefOf.EBSG_ReloadAbility, thing);
                            job.count = Mathf.Min(thing.stackCount, reloadComp.ChargesNeeded * reloadComp.Props.ammoPerCharge);
                            job.ability = ability;
                            return job;
                        }
                    }
                }

            return null;
        }
    }
}

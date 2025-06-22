using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class FloatMenuOptionProvider_ReloadableAbilities : FloatMenuOptionProvider
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

        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            if (Cache?.abilityFuel.NullOrEmpty() != false)
                return false;

            if (!context.FirstSelectedPawn.PawnHasAnyOfAbilities(Cache.reloadableAbilities, out _))
                return false;

            return true;
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            // Quick escape if no ability has the fuel
            if (!Cache.abilityFuel.Contains(clickedThing.def))
                return null;

            Pawn pawn = context.FirstSelectedPawn;

            if (context.FirstSelectedPawn.PawnHasAnyOfAbilities(Cache.reloadableAbilities, out var abilities))
                foreach (var ability in abilities)
                {
                    CompAbilityEffect_Reloadable reloadable = ability.CompOfType<CompAbilityEffect_Reloadable>();
                    if (reloadable.Props.ammoDef == clickedThing.def && !clickedThing.IsForbidden(pawn) && pawn.CanReserve(clickedThing))
                    {
                        string baseExplanation = "EBSG_Recharge".Translate(ability.def.LabelCap);
                        if (!pawn.CanReach(clickedThing, PathEndMode.OnCell, Danger.Deadly))
                            return new FloatMenuOption(baseExplanation + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                        else if (clickedThing.stackCount < reloadable.Props.ammoPerCharge)
                            return new FloatMenuOption(baseExplanation + ": " + "ReloadNotEnough".Translate().CapitalizeFirst(), null);
                        else if (reloadable.ChargesNeeded <= 0)
                            return new FloatMenuOption(baseExplanation + ": " + "ReloadFull".Translate(), null);
                        else
                            return new FloatMenuOption(baseExplanation, delegate
                            {
                                Job job = JobMaker.MakeJob(EBSGDefOf.EBSG_ReloadAbility, clickedThing);
                                job.count = Mathf.Min(clickedThing.stackCount, reloadable.ChargesNeeded * reloadable.Props.ammoPerCharge);
                                job.ability = ability;
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            });
                    }
                }

            return null;
        }
    }
}

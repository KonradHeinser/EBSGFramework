using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class JobDriver_ReloadAbility : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompAbilityEffect_Reloadable comp = job.ability.CompOfType<CompAbilityEffect_Reloadable>();

            this.FailOn(() => comp == null);
            this.FailOn(() => comp.parent.pawn != pawn);
            this.FailOn(() => comp.ChargesNeeded == 0);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            Toil getNextIngredient = Toils_General.Label();

            yield return getNextIngredient;
            foreach (Toil item in ReloadAsMuchAsPossible(comp))
            {
                yield return item;
            }

            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true).FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.A).NullOrEmpty());

            foreach (Toil item2 in ReloadAsMuchAsPossible(comp))
            {
                yield return item2;
            }

            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                if (carriedThing != null && !carriedThing.Destroyed)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }

        public IEnumerable<Toil> ReloadAsMuchAsPossible(CompAbilityEffect_Reloadable comp)
        {
            Toil done = Toils_General.Label();
            yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null || pawn.carryTracker.CarriedThing.stackCount < comp.Props.ammoPerCharge);
            yield return Toils_General.Wait(comp.Props.reloadDuration).WithProgressBarToilDelay(TargetIndex.A);
            Toil toil = ToilMaker.MakeToil("ReloadAsMuchAsPossible");
            toil.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                int charges = Mathf.Min(comp.ChargesNeeded, Mathf.FloorToInt(carriedThing.stackCount / comp.Props.ammoPerCharge));
                comp.RemainingCharges += charges;
                carriedThing.stackCount -= charges * comp.Props.ammoPerCharge;
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield return done;
        }
    }
}

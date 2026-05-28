using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace EBSGFramework
{
    public class JobDriver_Gatherer : JobDriver
    {
        private int progress; // Ticks already spent
        private int progressNeeded = 1500; // Ticks required
        public Thing GathererCenter => TargetA.Thing;
        public IntVec3 GathererDestination => TargetB.Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            CompGathererSpot comp = GathererCenter.TryGetComp<CompGathererSpot>();
            progressNeeded = comp.Props.ticksNeededToFindSomething.RandomInRange;

            if (GathererCenter.def.hasInteractionCell)
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            else if (GathererCenter.def.passability == Traversability.Standable)
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            else
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil prepare = new Toil
            {
                tickAction = () =>
                {
                    progress++;

                    if (progress % 300 == 0)
                    {
                        SetNextDestination();
                        ReadyForNextToil();
                    }
                },
                handlingFacing = false,
                defaultCompleteMode = ToilCompleteMode.Never
            };

            prepare.WithProgressBar(TargetIndex.A, () => (float)progress / (float)progressNeeded);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return prepare;
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

            Toil gathering = new Toil
            {
                tickAction = () =>
                {
                    progress++;

                    if (progress >= progressNeeded)
                        ReadyForNextToil();
                    else if (progress % 600 == 0) // If the duration is long enough, occassionally move to another location to make it a little less weird to watch
                    {
                        SetNextDestination();
                        pawn.pather.StartPath(pawn.jobs.curJob.GetTarget(TargetIndex.B), PathEndMode.OnCell);
                    }
                },
                handlingFacing = false
            };

            gathering.WithProgressBar(TargetIndex.A, () => (float)progress / (float)progressNeeded);
            gathering.defaultCompleteMode = ToilCompleteMode.Never;

            if (comp.Props.gatheringSound != null)
                gathering.PlaySustainerOrSound(comp.Props.gatheringSound);

            yield return gathering;

            if (GathererCenter.def.hasInteractionCell)
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            else
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);


            Toil finish = new Toil
            {
                initAction = () =>
                {
                    GatherOption option = comp.ViableOptions.RandomElementByWeight(o => o.weight);

                    if (option.thing != null)
                    {
                        int count = option.amountOnFind.RandomInRange;

                        if (count > 0)
                        {
                            Thing find = ThingMaker.MakeThing(option.thing, option.stuff);
                            find.stackCount = count;
                            GenSpawn.Spawn(find, pawn.Position, Map);
                        }
                    }

                    comp.Props.gatheringFinishedSound?.PlayOneShot(pawn);

                    ReadyForNextToil();
                }
            };

            yield return finish;
        }

        private void SetNextDestination()
        {
            CompGathererSpot comp = GathererCenter.TryGetComp<CompGathererSpot>();
            var wanderTiles = Map.AllCells.Where(p => p.DistanceTo(GathererCenter.Position) <= comp.Props.gatherRadius && !p.Impassable(Map)).ToList();

            if (!wanderTiles.NullOrEmpty())
                if (comp.Props.focusWanderInWater && wanderTiles.Where(t => t.GetTerrain(Map).IsWater).TryRandomElement(out var waterSpot))
                    job.SetTarget(TargetIndex.B, waterSpot);
                else if (wanderTiles.Where(t => !t.GetTerrain(Map).IsWater).TryRandomElement(out var drySpot))
                    job.SetTarget(TargetIndex.B, drySpot);
                else
                    job.SetTarget(TargetIndex.A, wanderTiles.RandomElement());
            else
                job.SetTarget(TargetIndex.B, GathererCenter.Position);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref progress, "progress");
            Scribe_Values.Look(ref progressNeeded, "progressNeeded", 1500);
        }

    }
}

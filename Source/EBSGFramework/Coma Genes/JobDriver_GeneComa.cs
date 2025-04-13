using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class JobDriver_GeneComa : JobDriver
    {
        private Building_Bed Bed => job.GetTarget(TargetIndex.A).Thing as Building_Bed;

        public override bool PlayerInterruptable => !OnLastToil;

        [Unsaved(false)]
        private Gene_Coma cachedComaGene;

        private Gene_Coma ComaGene
        {
            get
            {
                if (cachedComaGene == null)
                    cachedComaGene = pawn.genes?.GetGene(Extension.relatedGene) as Gene_Coma;

                return cachedComaGene;
            }
        }

        private ComaExtension cachedExtension;

        public ComaExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                    cachedExtension = job.def.GetModExtension<ComaExtension>();

                return cachedExtension;
            }
        }

        public override string GetReport()
        {
            if (ComaGene.Paused)
            {
                return "EBSG_ComaPaused".Translate(ComaGene.ComaExtension.noun) + ": " + "LethalInjuries".Translate();
            }

            TaggedString taggedString = "EBSG_Coma".Translate(ComaGene.ComaExtension.gerund).CapitalizeFirst() + ": ";
            float comaPercent = ComaGene.ComaPercent;
            if (comaPercent < 1f)
                taggedString += Mathf.Min(comaPercent, 0.99f).ToStringPercent("F0") + ", " +
                        "DurationLeft".Translate((ComaGene.MinComaTicks - ComaGene.comaRestTicks).ToStringTicksToPeriod());
            else
                taggedString += string.Format("{0} - {1}", "Complete".Translate().CapitalizeFirst(), "CanWakeSafely".Translate());
            return taggedString.Resolve();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Bed != null)
                return pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);

            return true;
        }

        public override bool CanBeginNowWhileLyingDown()
        {
            return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool hasBed = Bed != null;
            if (hasBed)
            {
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
                Toil toil = Toils_Bed.GotoBed(TargetIndex.A);

                if (ComaGene.ComaExtension.needBedOutOfSunlight)
                    toil.AddFailCondition(delegate
                    {
                        if (!ComaGene.Paused)
                            foreach (IntVec3 item in Bed.OccupiedRect())
                                if (item.InSunlight(Bed.Map))
                                {
                                    Messages.Message("MessageBedExposedToSunlight".Translate(pawn.Named("PAWN"), Bed.Named("BED")), Bed, MessageTypeDefOf.RejectInput);
                                    return true;
                                }

                        return false;
                    });

                yield return toil;
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            }

            Toil toil2 = Toils_LayDown.LayDown(TargetIndex.A, hasBed, false, true, true, PawnPosture.LayingOnGroundNormal, false);
            toil2.initAction = (Action)Delegate.Combine(toil2.initAction, (Action)delegate
            {
                if (pawn.Drafted)
                    pawn.drafter.Drafted = false;
                ComaGene.Notify_ComaStarted();
            });

            if (ComaGene.ComaExtension.restingMote != null)
                toil2.tickAction = (Action)Delegate.Combine(toil2.tickAction, (Action)delegate
                {
                    if (pawn.IsHashIntervalTick(160))
                        MoteMaker.MakeAttachedOverlay(pawn, ComaGene.ComaExtension.restingMote, new Vector3(0f, pawn.story.bodyType.bedOffset).RotatedBy(pawn.Rotation));
                });

            yield return toil2;
        }
    }
}

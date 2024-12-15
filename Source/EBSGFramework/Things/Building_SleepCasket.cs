using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace EBSGFramework
{
    public class Building_SleepCasket : Building_Casket
    {
        private static List<ThingDef> cachedCaskets;

        private EBSGExtension cachedExtension;

        private bool checkedExtension;

        public EBSGExtension Extension
        {
            get
            {
                if (cachedExtension == null && !checkedExtension)
                {
                    cachedExtension = def.GetModExtension<EBSGExtension>();
                    checkedExtension = true;
                }
                return cachedExtension;
            }
        }

        public ThingOwner InnerContainer => innerContainer;

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects && Extension?.startSound != null)
                {
                    Extension.startSound.PlayOneShot(new TargetInfo(Position, Map));
                }
                return true;
            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.IsQuestLodger())
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
                yield break;
            }
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }
            if (innerContainer.Count != 0)
            {
                yield break;
            }
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield break;
            }
            JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
            string label = "EBSG_EnterSleepCasket".Translate(def.label);
            Action action = delegate
            {
                if (ModsConfig.BiotechActive)
                {
                    if (!(selPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff_PsychicBond) || !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(selPawn, hediff_PsychicBond))
                    {
                        selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
                    }
                    else
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("EBSG_PsychicBondDistanceWillBeActive".Translate(selPawn.Named("PAWN"), ((Pawn)hediff_PsychicBond.target).Named("BOND")), delegate
                        {
                            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
                        }, true));
                    }
                }
                else
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
                }
            };
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), selPawn, this);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (Faction == Faction.OfPlayer && innerContainer.Count > 0 && def.building.isPlayerEjectable)
            {
                Command_Action command_Action = new Command_Action
                {
                    action = EjectContents,
                    defaultLabel = "CommandPodEject".Translate(),
                    defaultDesc = "EBSG_CommandSleepCasketEjectDesc".Translate()
                };
                if (innerContainer.Count == 0)
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());

                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
                yield return command_Action;
            }
        }

        public override void EjectContents()
        {
            foreach (Thing item in innerContainer)
            {
                if (item is Pawn pawn)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    if (Extension != null)
                    {
                        if (Extension.filth != null)
                            pawn.filth.GainFilth(Extension.filth);
                        if (pawn.RaceProps.IsFlesh && Extension.endHediff != null)
                            EBSGUtilities.AddOrAppendHediffs(pawn, hediff: Extension.endHediff);
                        else if (pawn.RaceProps.IsMechanoid && Extension.mechEndHediff != null)
                            EBSGUtilities.AddOrAppendHediffs(pawn, hediff: Extension.mechEndHediff);
                    }

                }
            }
            if (!Destroyed && Extension?.endSound != null)
                Extension.endSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(Position, Map)));

            base.EjectContents();
        }

        public static Building_SleepCasket FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            if (cachedCaskets == null)
                cachedCaskets = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => typeof(Building_SleepCasket).IsAssignableFrom(def.thingClass)).ToList();

            foreach (ThingDef cachedCasket in cachedCaskets)
            {
                bool queuing = KeyBindingDefOf.QueueOrder.IsDownEvent;
                Building_SleepCasket building_CryptosleepCasket = (Building_SleepCasket)GenClosest.ClosestThingReachable(p.PositionHeld, p.MapHeld, ThingRequest.ForDef(cachedCasket), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, Validator);
                if (building_CryptosleepCasket != null)
                    return building_CryptosleepCasket;

                bool Validator(Thing x)
                {
                    if (!((Building_SleepCasket)x).HasAnyContents && (!queuing || !traveler.HasReserved(x)))
                        return traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations);
                    return false;
                }
            }
            return null;
        }
    }
}

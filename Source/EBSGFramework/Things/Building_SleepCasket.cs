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
    public class Building_SleepCasket : Building_Casket, ISuspendableThingHolder, IStoreSettingsParent
    {
        private static List<ThingDef> cachedCaskets;

        private EBSGExtension cachedExtension;

        private bool checkedExtension;

        private float storedNutrition;

        public float NutritionStored
        {
            get
            {
                float num = storedNutrition;
                for (int i = 0; i < innerContainer.Count; i++)
                {
                    Thing thing = innerContainer[i];
                    num += (float)thing.stackCount * thing.GetStatValue(StatDefOf.Nutrition);
                }
                return num;
            }
        }

        public float NutritionNeeded
        {
            get
            {
                if (def.building.defaultStorageSettings == null) return 0f;
                if (!PawnInside)
                {
                    return Mathf.Max(0, (MaxStorage * 0.25f) - NutritionStored);
                }
                return MaxStorage - NutritionStored;
            }
        }

        public float MaxStorage => Extension?.maxStorage ?? 10f;

        public bool PawnInside
        {
            get
            {
                if (innerContainer.Count <= 0) return false;
                foreach (Thing thing in innerContainer)
                    if (thing is Pawn) return true;
                return false;
            }
        }

        public float NutritionConsumedPerTick
        {
            get
            {
                float num = 0f;
                if (PawnInside)
                {
                    if (Extension?.staticUsePerDay > 0f)
                        num = Extension.staticUsePerDay;
                    else
                        foreach (Thing thing in innerContainer)
                            if (thing is Pawn pawn)
                            {
                                num += pawn.needs.food.FoodFallPerTick;
                                if (EBSGUtilities.HasHediff(pawn, HediffDefOf.Lactating))
                                    num -= pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating).TryGetComp<HediffComp_Lactating>().AddedNutritionPerDay() / 60000f;
                            }
                }
                return num;
            }
        }

        private StorageSettings allowedNutritionSettings;

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

        public bool IsContentsSuspended => (def.building.defaultStorageSettings == null || storedNutrition > 0) && GetComp<CompRefuelable>()?.HasFuel != false &&
            (PowerComp == null || PowerComp.TransmitsPowerNow || (PowerComp.PowerNet != null && PowerComp.PowerNet.CurrentStoredEnergy() > 0));

        public bool StorageTabVisible => true;

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

        public override void PostMake()
        {
            base.PostMake();
            allowedNutritionSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                allowedNutritionSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (def.building.defaultStorageSettings != null)
            {
                storedNutrition = Mathf.Clamp(storedNutrition - NutritionConsumedPerTick, 0f, 2.1474836E+09f);
                if (storedNutrition <= 0f)
                    TryAbsorbNutritiousThing();
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.IsQuestLodger())
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
                yield break;
            }
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
                yield return floatMenuOption;

            if (PawnInside)
                yield break;

            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield break;
            }
            JobDef jobDef = Extension?.relatedJob ?? EBSGDefOf.EBSG_EnterSleepCasket;
            string label = "EBSG_EnterSleepCasket".Translate(def.label);
            Action action = delegate
            {
                if (ModsConfig.BiotechActive)
                {
                    if (!(selPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff_PsychicBond) || !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(selPawn, hediff_PsychicBond))
                        selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, this), JobTag.Misc);
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
            if (Faction == Faction.OfPlayer && innerContainer.Count != 0 && def.building.isPlayerEjectable)
            {
                Command_Action command_Action = new Command_Action
                {
                    action = EjectContents,
                    defaultLabel = "CommandPodEject".Translate(),
                    defaultDesc = "EBSG_CommandSleepCasketEjectDesc".Translate()
                };
                if (!PawnInside)
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());

                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
                yield return command_Action;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Fill nutrition",
                    action = delegate
                    {
                        storedNutrition = MaxStorage;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Empty nutrition",
                    action = delegate
                    {
                        storedNutrition = 0f;
                    }
                };
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
                            pawn.AddHediffToParts(null, Extension.endHediff);
                        else if (pawn.RaceProps.IsMechanoid && Extension.mechEndHediff != null)
                            pawn.AddHediffToParts(null, Extension.mechEndHediff);
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

        public bool CanAcceptNutrition(Thing thing)
        {
            return allowedNutritionSettings.AllowedToAccept(thing);
        }

        public StorageSettings GetStoreSettings()
        {
            return allowedNutritionSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        {
        }

        private void TryAbsorbNutritiousThing()
        {
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (!(innerContainer[i] is Pawn))
                {
                    float statValue = innerContainer[i].GetStatValue(StatDefOf.Nutrition);
                    if (statValue > 0f)
                    {
                        storedNutrition += statValue;
                        innerContainer[i].SplitOff(1).Destroy();
                        break;
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storedNutrition, "storedNutrition", 0f);
            Scribe_Deep.Look(ref allowedNutritionSettings, "allowedNutritionSettings", this);
            if (allowedNutritionSettings == null)
            {
                allowedNutritionSettings = new StorageSettings(this);
                if (def.building.defaultStorageSettings != null)
                    allowedNutritionSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }
    }
}

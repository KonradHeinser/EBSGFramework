using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Gene_Coma : HediffAdder
    {
        public bool alreadyChecked;

        private ComaExtension cachedComaExtension;

        public ComaExtension ComaExtension
        {
            get
            {
                if (cachedComaExtension == null && !alreadyChecked)
                {
                    cachedComaExtension = def.GetModExtension<ComaExtension>();
                    alreadyChecked = true;
                }

                return cachedComaExtension;
            }
        }

        private GeneGizmo_ComaRestCapacity gizmo;

        private CachedTexture WakeCommandTex => GetWakeCommandTex();

        private CachedTexture AutoWakeCommandTex => GetAutoWakeCommandTex();

        private CachedTexture GetWakeCommandTex()
        {
            if (ComaExtension.wakeCommandTexPath != null)
                return new CachedTexture(ComaExtension.wakeCommandTexPath);
            return new CachedTexture("UI/Gizmos/Wake");
        }

        private CachedTexture GetAutoWakeCommandTex()
        {
            if (ComaExtension.autoWakeTexCommandPath != null)
                return new CachedTexture(ComaExtension.autoWakeTexCommandPath);
            return new CachedTexture("UI/Gizmos/DeathrestAutoWake");
        }

        private int comaRestCapacity;

        private List<Thing> boundBuildings = new List<Thing>();

        public float adjustedComaTicks;

        public int comaRestTicks;

        public bool autoWake;

        private bool notifiedWakeOK;

        [Unsaved(false)]
        private Need_ComaGene cachedComaNeed;

        [Unsaved(false)]
        private List<CompComaGeneBindable> cachedBoundComps;

        public const float PresencePercentRequiredToApply = 0.75f;

        public Need_ComaGene ComaNeed
        {
            get
            {
                if (cachedComaNeed == null)
                {
                    cachedComaNeed = pawn.needs?.TryGetNeed<Need_ComaGene>();

                    // This is for the off chance that my warning about making coma genes exclusive was ignored
                    if (cachedComaNeed != null && cachedComaNeed.ComaGene != this)
                    {
                        cachedComaNeed = null;
                        foreach (Need need in pawn.needs.AllNeeds)
                            if (need is Need_ComaGene comaNeed && comaNeed.ComaGene == this)
                            {
                                cachedComaNeed = comaNeed;
                                break;
                            }
                    }
                }

                return cachedComaNeed;
            }
        }

        public List<Thing> BoundBuildings => boundBuildings;

        public List<HediffDef> temporaryHediffs;

        public List<CompComaGeneBindable> BoundComps
        {
            get
            {
                if (cachedBoundComps == null)
                {
                    cachedBoundComps = new List<CompComaGeneBindable>();
                    foreach (Thing boundBuilding in boundBuildings)
                    {
                        if (CanUseBindableNow(boundBuilding))
                        {
                            cachedBoundComps.Add(boundBuilding.TryGetComp<CompComaGeneBindable>());
                        }
                    }
                }
                return cachedBoundComps;
            }
        }

        public float ComaEfficiency
        {
            get
            {
                float num = ComaNeed.RiseMultiplier;
                foreach (CompComaGeneBindable boundComp in BoundComps)
                    if (boundComp.Props.comaRestEffectivenessFactor > 0f && boundComp.CanIncreasePresence)
                        num *= boundComp.Props.comaRestEffectivenessFactor;
                return num;
            }
        }

        public int MinComaTicks => Mathf.RoundToInt(ComaExtension.minComaTicks / ComaEfficiency);

        public int ComaCapacity => comaRestCapacity;

        public int CurrentCapacity
        {
            get
            {
                int num = 0;
                for (int i = 0; i < boundBuildings.Count; i++)
                {
                    CompComaGeneBindable compComaBindable = boundBuildings[i].TryGetComp<CompComaGeneBindable>();
                    if (compComaBindable != null && compComaBindable.Props.countsTowardsBuildingLimit)
                        num++;
                }
                return num;
            }
        }

        public Thing BoundBed
        {
            get
            {
                if (!boundBuildings.NullOrEmpty())
                    foreach (Thing thing in boundBuildings)
                        if (thing.def.IsBed) return thing;
                return null;
            }
        }

        public bool AtBuildingCapacityLimit => CurrentCapacity >= comaRestCapacity;

        public float ComaPercent => Mathf.Clamp01((float)comaRestTicks / (float)MinComaTicks);

        public bool ShowWakeAlert
        {
            get
            {
                if (ComaPercent >= 1f)
                    return !autoWake;

                return false;
            }
        }

        private int lastPauseCheckTick = -1;

        private bool cachedPaused;

        public bool Paused
        {
            get
            {
                if (lastPauseCheckTick < Find.TickManager.TicksGame + 120)
                {
                    lastPauseCheckTick = Find.TickManager.TicksGame;
                    cachedPaused = SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(pawn);
                }
                return cachedPaused;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            Reset();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            pawn.RemoveHediffs(ComaExtension.comaRestingHediff);
            pawn.RemoveHediffs(ComaExtension.comaInterruptedHediff);
            pawn.RemoveHediffs(ComaExtension.exhaustionHediff);
            RemoveOldComaBonuses();
            Reset();
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.IsHashIntervalTick(200) && pawn.IsColonistPlayerControlled)
                LessonAutoActivator.TeachOpportunity(EBSGDefOf.EBSG_SpecialComa, OpportunityType.Important);

            if (Paused) return;

            ComaNeed.lastComaTick = Find.TickManager.TicksGame;

            comaRestTicks += delta;
            adjustedComaTicks += ComaEfficiency * delta;
            if (!BoundComps.NullOrEmpty())
                foreach (CompComaGeneBindable boundComp in BoundComps)
                    boundComp.TryIncreasePresence();

            if (ComaPercent >= 1f)
            {
                if (autoWake)
                {
                    Wake();
                    return;
                }

                if (!notifiedWakeOK && PawnUtility.ShouldSendNotificationAbout(pawn))
                    Messages.Message("EBSG_ComaSafeWake".Translate(ComaExtension.noun, pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);

                notifiedWakeOK = true;
            }

            if (ComaExtension.needBedOutOfSunlight && pawn.Spawned && pawn.IsHashIntervalTick(100) && PawnOrBedTouchingSunlight())
            {
                if (PawnUtility.ShouldSendNotificationAbout(pawn))
                    Messages.Message("EBSG_ComaSunlightWake".Translate(ComaExtension.noun, pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
                Wake();
            }
        }

        public void OffsetCapacity(int offset, bool sendNotification = true)
        {
            int num = comaRestCapacity;
            comaRestCapacity += offset;
            if (sendNotification && PawnUtility.ShouldSendNotificationAbout(pawn))
                Messages.Message("EBSG_ComaCapacityChanged".Translate(ComaExtension.noun, pawn.Named("PAWN"), num.Named("OLD"), comaRestCapacity.Named("NEW")), pawn, MessageTypeDefOf.PositiveEvent);
        }

        private bool PawnOrBedTouchingSunlight()
        {
            Building_Bed building_Bed = pawn.CurrentBed();
            if (building_Bed != null)
                foreach (IntVec3 item in building_Bed.OccupiedRect())
                    if (item.InSunlight(building_Bed.Map))
                        return true;

            return false;
        }

        public void Notify_ComaStarted()
        {
            pawn.health.AddHediff(ComaExtension.comaRestingHediff);
            pawn.RemoveHediffs(ComaExtension.comaInterruptedHediff);
            pawn.RemoveHediffs(ComaExtension.exhaustionHediff);
            if (ComaExtension.usesBuildings)
            {
                RemoveOldComaBonuses();
                TryLinkToNearbyComaBuildings();
            }
            notifiedWakeOK = false;
        }

        public void Notify_BoundBuildingDeSpawned(Thing building)
        {
            boundBuildings.Remove(building);
            pawn.ownership.UnclaimDeathrestCasket();
            if (building is Building_Bed)
                Wake();
            cachedBoundComps = null;
        }

        public void TryLinkToNearbyComaBuildings()
        {
            if (!ModsConfig.BiotechActive || !pawn.Spawned)
                return;

            cachedBoundComps = null;
            Room room = pawn.GetRoom();
            if (room == null)
                return;

            foreach (Region region in room.Regions)
                foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
                {
                    CompComaGeneBindable bindComp = item.TryGetComp<CompComaGeneBindable>();
                    if (CanBindToBindable(bindComp))
                        BindTo(bindComp);
                }
        }

        public void RemoveOldComaBonuses()
        {
            pawn.RemoveHediffs(null, temporaryHediffs);
            pawn.genes.GetFirstGeneOfType<Gene_Hemogen>()?.ResetMax();
        }

        private void ApplyComaBuildingBonuses()
        {
            if (!ComaExtension.usesBuildings || comaRestTicks == 0)
                return;

            temporaryHediffs = new List<HediffDef>();

            if (!BoundComps.NullOrEmpty())
                foreach (CompComaGeneBindable boundComp in BoundComps)
                    if ((float)boundComp.presenceTicks / (float)comaRestTicks >= 0.75f)
                        boundComp.Apply();
        }

        public bool CanBindToBindable(CompComaGeneBindable bindComp)
        {
            if (bindComp == null || bindComp.parent.Faction != pawn.Faction || boundBuildings.Contains(bindComp.parent) ||
                (bindComp.Props.countsTowardsBuildingLimit && AtBuildingCapacityLimit) || !bindComp.CanBindTo(pawn) || !CanUseBindableNow(bindComp.parent) ||
                BindingWillExceedStackLimit(bindComp)) return false;
            return true;
        }

        public bool BindingWillExceedStackLimit(CompComaGeneBindable bindComp)
        {
            if (boundBuildings.Contains(bindComp.parent) || !bindComp.Props.countsTowardsBuildingLimit)
                return false;

            int stackLimit = bindComp.Props.stackLimit;
            if (stackLimit > 0)
            {
                int num = 0;
                for (int i = 0; i < boundBuildings.Count; i++)
                {
                    if (boundBuildings[i].def == bindComp.parent.def)
                    {
                        num++;
                        if (num >= stackLimit)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool CanUseBindableNow(Thing building)
        {
            CompComaGeneBindable compComaBindable = building.TryGetComp<CompComaGeneBindable>();
            if (compComaBindable == null)
                return false;

            Building_Bed building_Bed = pawn.CurrentBed();
            if (building_Bed == null)
                return false;

            if (building is Building_Bed)
            {
                if (building_Bed != building)
                    return false;
            }
            else if (!building_Bed.def.HasComp(typeof(CompComaGeneBindable)))
            {
                return false;
            }
            if (!building.Spawned || building.Map != pawn.MapHeld || building.GetRoom() != pawn.GetRoom())
                return false;

            if (!GenSight.LineOfSight(pawn.Position, building.OccupiedRect().CenterCell, pawn.Map))
                return false;

            return true;
        }

        public void BindTo(CompComaGeneBindable bindComp)
        {
            bindComp.BindTo(pawn);
            boundBuildings.Add(bindComp.parent);
            cachedBoundComps = null;
            if (PawnUtility.ShouldSendNotificationAbout(pawn) && bindComp.Props.countsTowardsBuildingLimit)
                Messages.Message("EBSG_ComaBuildingBound".Translate(ComaExtension.noun, bindComp.parent.Named("BUILDING"), pawn.Named("PAWN"), CurrentCapacity.Named("CUR"), ComaCapacity.Named("MAX")), new LookTargets(bindComp.parent, pawn), MessageTypeDefOf.NeutralEvent, historical: false);
        }

        public override void Reset()
        {
            comaRestCapacity = ComaExtension.baseBuildingMax;
            if (!BoundComps.NullOrEmpty())
                foreach (CompComaGeneBindable boundComp in BoundComps)
                    boundComp.Notify_ComaGeneRemoved();

            cachedBoundComps = null;
            if (pawn.CurJobDef == ComaExtension.relatedJob)
                pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
        }

        public override void Notify_NewColony()
        {
            boundBuildings.Clear();
            cachedBoundComps = null;
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            boundBuildings.Clear();
            cachedBoundComps = null;
            cachedComaNeed = null;
        }

        public void Wake()
        {
            if (ComaPercent < 1f)
            {
                if (ComaExtension.comaInterruptedHediff != null)
                    pawn.health.AddHediff(ComaExtension.comaInterruptedHediff);
            }
            else
                ApplyComaBuildingBonuses();

            if (ComaExtension.comaRestingHediff != null)
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(ComaExtension.comaRestingHediff);
                if (firstHediffOfDef != null)
                    pawn.health.RemoveHediff(firstHediffOfDef);
            }
            comaRestTicks = 0;
            adjustedComaTicks = 0f;

            foreach (Thing boundBuilding in boundBuildings)
                boundBuilding.TryGetComp<CompComaGeneBindable>().Notify_ComaEnded();
        }

        private bool shownMissingNeedWarning = false;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!Active)
                yield break;

            if (gizmo == null && ComaExtension.usesBuildings)
                gizmo = new GeneGizmo_ComaRestCapacity(this);

            if (gizmo != null && Find.Selector.SelectedPawns.Count == 1)
                yield return gizmo;

            if (ComaNeed == null)
            {
                if (!shownMissingNeedWarning)
                {
                    Log.Error($"{def.defName} is missing a linked need. This should be added through enablesNeeds.");
                    shownMissingNeedWarning = true;
                }
                yield break;
            }

            if (ComaNeed.Comatose)
            {
                if (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony)
                {
                    string text = "EBSG_ComaWakeDesc".Translate(ComaExtension.noun, pawn.Named("PAWN"), comaRestTicks.ToStringTicksToPeriod().Named("DURATION")).Resolve() + "\n\n";
                    text += (!(ComaPercent < 1f)) ? "WakeExtraDesc_Safe".Translate(pawn.Named("PAWN")).Resolve() : "EBSG_ComaWakeExtraDesc_Exhaustion".Translate(ComaExtension.noun, pawn.Named("PAWN"), MinComaTicks.ToStringTicksToPeriod().Named("TOTAL")).Resolve();
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Wake".Translate().CapitalizeFirst(),
                        defaultDesc = text,
                        icon = WakeCommandTex.Texture,
                        action = delegate
                        {
                            if (ComaPercent < 1f)
                            {
                                string warning = "EBSG_WarningWakingInterruptsComa".Translate(ComaExtension.noun, ComaExtension.gerund, pawn.Named("PAWN"), MinComaTicks.ToStringTicksToPeriod().Named("MINDURATION"), comaRestTicks.ToStringTicksToPeriod().Named("CURDURATION"));
                                if (!BoundBuildings.NullOrEmpty()) warning += "EBSG_ComaNoBuildingBonusWarning".Translate(ComaExtension.noun);
                                Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation(warning, Wake, true);
                                Find.WindowStack.Add(window);
                            }
                            else
                            {
                                Wake();
                            }
                        }
                    };
                    yield return command_Action;
                    if (ComaPercent < 1f && ComaExtension.minComaTicks > 0)
                    {
                        yield return new Command_Toggle
                        {
                            defaultLabel = "AutoWake".Translate().CapitalizeFirst(),
                            defaultDesc = "EBSG_ComaAutoWakeDesc".Translate(ComaExtension.noun, ComaExtension.gerund, pawn.Named("PAWN")).Resolve(),
                            icon = AutoWakeCommandTex.Texture,
                            isActive = () => autoWake,
                            toggleAction = delegate
                            {
                                autoWake = !autoWake;
                            }
                        };
                    }
                }
                if (DebugSettings.ShowDevGizmos)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Wake and apply bonuses",
                        action = delegate
                        {
                            comaRestTicks = MinComaTicks + 1;
                            adjustedComaTicks = ComaExtension.minComaTicks + 1;
                            foreach (CompComaGeneBindable boundComp in BoundComps)
                                boundComp.presenceTicks = comaRestTicks;

                            Wake();
                        }
                    };
                }
            }

            if (!DebugSettings.ShowDevGizmos || !ComaExtension.usesBuildings)
                yield break;

            yield return new Command_Action
            {
                defaultLabel = "DEV: Set capacity",
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    for (int i = 1; i <= 20; i++)
                    {
                        int newCap = i;
                        list.Add(new FloatMenuOption(newCap.ToString(), delegate
                        {
                            OffsetCapacity(newCap - comaRestCapacity, false);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            };
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "EBSG_Capacity".Translate(ComaExtension.noun).CapitalizeFirst(), comaRestCapacity.ToString(), "EBSG_ComaRestCapacityDesc".Translate(ComaExtension.gerund), 1010);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref comaRestCapacity, "comaRestCapacity", 1);
            Scribe_Values.Look(ref adjustedComaTicks, "adjustedComaTicks", 0f);
            Scribe_Values.Look(ref comaRestTicks, "comaRestTicks", 0);
            Scribe_Values.Look(ref autoWake, "autoWake", false);
            Scribe_Values.Look(ref notifiedWakeOK, "notifiedWakeOK", false);
            Scribe_Collections.Look(ref boundBuildings, "boundBuildings", LookMode.Reference);
            Scribe_Collections.Look(ref temporaryHediffs, "temporaryHediffs", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                boundBuildings.RemoveAll((Thing x) => x == null);
        }
    }
}

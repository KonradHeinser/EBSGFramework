using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Gene_Coma : Gene
    {
        public bool alreadyChecked;

        private EBSGExtension cachedExtension;

        public EBSGExtension Extension
        {
            get
            {
                if (cachedExtension == null && !alreadyChecked)
                {
                    cachedExtension = def.GetModExtension<EBSGExtension>();
                    alreadyChecked = true;
                }

                return cachedExtension;
            }
        }

        private GeneGizmo_ComaRestCapacity gizmo;

        private static readonly CachedTexture WakeCommandTex = new CachedTexture("UI/Gizmos/Wake");

        private static readonly CachedTexture AutoWakeCommandTex = new CachedTexture("UI/Gizmos/DeathrestAutoWake");

        private int comaRestCapacity;

        private List<Thing> boundBuildings = new List<Thing>();

        public float adjustedComaTicks;

        public int comaRestTicks;

        public bool autoWake;

        public int chamberThoughtIndex = -1;

        private bool notifiedWakeOK;

        [Unsaved(false)]
        private Need_ComaGene cachedComaNeed;

        [Unsaved(false)]
        private List<CompComaGeneBindable> cachedBoundComps;

        private const int InitialComaCapacity = 1;

        public const int BaseComaTicksWithoutInterruptedHediff = 240000;

        public const float PresencePercentRequiredToApply = 0.75f;

        private const int LessonComaTicks = 200;

        private const int SunlightCheckInterval = 150;

        public Need_ComaGene ComaNeed
        {
            get
            {
                if (cachedComaNeed == null)
                {
                    cachedComaNeed = pawn.needs.TryGetNeed<Need_ComaGene>();
                }
                return cachedComaNeed;
            }
        }

        public List<Thing> BoundBuildings => boundBuildings;

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
                float num = 1f;
                foreach (CompComaGeneBindable boundComp in BoundComps)
                {
                    if (boundComp.Props.comaRestEffectivenessFactor > 0f && boundComp.CanIncreasePresence)
                    {
                        num *= boundComp.Props.comaRestEffectivenessFactor;
                    }
                }
                return num;
            }
        }

        public int MinComaTicks => Mathf.RoundToInt(240000f / ComaEfficiency);

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
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        public bool AtBuildingCapacityLimit => CurrentCapacity >= comaRestCapacity;

        public float ComaPercent => Mathf.Clamp01((float)comaRestTicks / (float)MinComaTicks);

        public bool ShowWakeAlert
        {
            get
            {
                if (ComaPercent >= 1f)
                {
                    return !autoWake;
                }
                return false;
            }
        }

        public override void PostAdd()
        {
            if (ModLister.CheckBiotech("Coma"))
            {
                base.PostAdd();
                Reset();
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Extension.comaRestingHediff);
            if (firstHediffOfDef != null)
            {
                pawn.health.RemoveHediff(firstHediffOfDef);
            }

            if (Extension.exhaustionHediff != null)
            {
                Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(Extension.exhaustionHediff);
                if (firstHediffOfDef2 != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef2);
                }
            }
            RemoveOldComaBonuses();
            Reset();
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(200) && pawn.IsColonistPlayerControlled)
            {
                LessonAutoActivator.TeachOpportunity(EBSGDefOf.EBSG_SpecialComa, OpportunityType.Important);
            }
        }

        public void OffsetCapacity(int offset, bool sendNotification = true)
        {
            int num = comaRestCapacity;
            comaRestCapacity += offset;
            if (sendNotification && PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message("MessageComaCapacityChanged".Translate(pawn.Named("PAWN"), num.Named("OLD"), comaRestCapacity.Named("NEW")), pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        public void TickComaing(bool paused)
        {
            if (paused)
            {
                return;
            }
            if (ComaNeed != null)
            {
                ComaNeed.lastComaTick = Find.TickManager.TicksGame;
            }
            comaRestTicks++;
            adjustedComaTicks += ComaEfficiency;
            foreach (CompComaGeneBindable boundComp in BoundComps)
            {
                boundComp.TryIncreasePresence();
            }
            if (ComaPercent >= 1f && !notifiedWakeOK)
            {
                notifiedWakeOK = true;
                if (autoWake)
                {
                    Wake();
                    return;
                }
                if (PawnUtility.ShouldSendNotificationAbout(pawn))
                {
                    Messages.Message("MessageComaingPawnCanWakeSafely".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
            if (pawn.Spawned && pawn.IsHashIntervalTick(150) && PawnOrBedTouchingSunlight())
            {
                if (PawnUtility.ShouldSendNotificationAbout(pawn))
                {
                    Messages.Message("MessagePawnWokenFromSunlight".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
                }
                Wake();
            }
        }

        private bool PawnOrBedTouchingSunlight()
        {
            Building_Bed building_Bed = pawn.CurrentBed();
            if (building_Bed != null)
            {
                foreach (IntVec3 item in building_Bed.OccupiedRect())
                {
                    if (item.InSunlight(building_Bed.Map))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Notify_ComaStarted()
        {
            RemoveOldComaBonuses();
            TryLinkToNearbyComaBuildings();
            notifiedWakeOK = false;
        }

        public void Notify_ComaEnded()
        {
            foreach (Thing boundBuilding in boundBuildings)
                boundBuilding.TryGetComp<CompComaGeneBindable>().Notify_ComaEnded();
        }

        public void Notify_BoundBuildingDeSpawned(Thing building)
        {
            boundBuildings.Remove(building);
            cachedBoundComps = null;
        }

        public void TryLinkToNearbyComaBuildings()
        {
            if (!ModsConfig.BiotechActive || !pawn.Spawned)
                return;

            cachedBoundComps = null;
            Room room = pawn.GetRoom();
            if (room == null)
            {
                return;
            }
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
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int num = hediffs.Count - 1; num >= 0; num--)
            {
                if (hediffs[num].def.removeOnComaStart) // Need to make special hediff comp that checks for certain genes
                {
                    pawn.health.RemoveHediff(hediffs[num]);
                }
            }
            pawn.genes.GetFirstGeneOfType<Gene_Hemogen>()?.ResetMax();
        }

        private void ApplyComaBuildingBonuses()
        {
            if (comaRestTicks == 0)
                return;

            if (Extension.comaInterruptedHediff != null)
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Extension.comaInterruptedHediff);
                if (firstHediffOfDef != null)
                    pawn.health.RemoveHediff(firstHediffOfDef);
            }

            if (!BoundComps.NullOrEmpty())
                foreach (CompComaGeneBindable boundComp in BoundComps)
                    if ((float)boundComp.presenceTicks / (float)comaRestTicks >= 0.75f)
                        boundComp.Apply();
        }

        public bool CanBindToBindable(CompComaGeneBindable bindComp)
        {
            if (bindComp == null || bindComp.parent.Faction != pawn.Faction || boundBuildings.Contains(bindComp.parent) ||
                bindComp.Props.countsTowardsBuildingLimit && AtBuildingCapacityLimit || !bindComp.CanBindTo(pawn) || !CanUseBindableNow(bindComp.parent) ||
                BindingWillExceedStackLimit(bindComp)) return false;
            return true;
        }

        public bool BindingWillExceedStackLimit(CompComaGeneBindable bindComp)
        {
            if (boundBuildings.Contains(bindComp.parent))
            {
                return false;
            }
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
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            CompComaGeneBindable compComaBindable = building.TryGetComp<CompComaGeneBindable>();
            if (compComaBindable == null)
            {
                return false;
            }
            Building_Bed building_Bed = pawn.CurrentBed();
            if (building_Bed == null)
            {
                return false;
            }
            if (compComaBindable.Props.mustBeLayingInToBind)
            {
                if (building_Bed != building)
                {
                    return false;
                }
            }
            else if (!building_Bed.def.HasComp(typeof(CompComaGeneBindable)))
            {
                return false;
            }
            if (!building.Spawned || building.Map != pawn.MapHeld || building.GetRoom() != pawn.GetRoom())
            {
                return false;
            }
            if (!GenSight.LineOfSight(pawn.Position, building.OccupiedRect().CenterCell, pawn.Map))
            {
                return false;
            }
            return true;
        }

        public void BindTo(CompComaGeneBindable bindComp)
        {
            bindComp.BindTo(pawn);
            boundBuildings.Add(bindComp.parent);
            cachedBoundComps = null;
            if (PawnUtility.ShouldSendNotificationAbout(pawn) && bindComp.Props.countsTowardsBuildingLimit)
            {
                Messages.Message("MessageComaBuildingBound".Translate(bindComp.parent.Named("BUILDING"), pawn.Named("PAWN"), CurrentCapacity.Named("CUR"), ComaCapacity.Named("MAX")), new LookTargets(bindComp.parent, pawn), MessageTypeDefOf.NeutralEvent, historical: false);
            }
        }

        public override void Reset()
        {
            comaRestCapacity = 1;
            foreach (CompComaGeneBindable boundComp in BoundComps)
            {
                boundComp.Notify_ComaGeneRemoved();
            }
            cachedBoundComps = null;
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
                if (Extension.comaInterruptedHediff != null)
                    pawn.health.AddHediff(Extension.comaInterruptedHediff);
            }
            else
            {
                ApplyComaBuildingBonuses();
            }
            if (Extension.comaRestingHediff != null)
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Extension.comaRestingHediff);
                if (firstHediffOfDef != null)
                    pawn.health.RemoveHediff(firstHediffOfDef);
            }
            comaRestTicks = 0;
            adjustedComaTicks = 0f;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!Active)
                yield break;

            if (gizmo == null)
            {
                gizmo = new GeneGizmo_ComaRestCapacity(this);
            }
            if (Find.Selector.SelectedPawns.Count == 1)
            {
                yield return gizmo;
            }
            if (ComaNeed.Comatose)
            {
                if (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony)
                {
                    string text = "WakeDesc".Translate(pawn.Named("PAWN"), comaRestTicks.ToStringTicksToPeriod().Named("DURATION")).Resolve() + "\n\n";
                    text = ((!(ComaPercent < 1f)) ? (text + "WakeExtraDesc_Safe".Translate(pawn.Named("PAWN")).Resolve()) : (text + "WakeExtraDesc_Exhaustion".Translate(pawn.Named("PAWN"), MinComaTicks.ToStringTicksToPeriod().Named("TOTAL")).Resolve()));
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Wake".Translate().CapitalizeFirst(),
                        defaultDesc = text,
                        icon = WakeCommandTex.Texture,
                        action = delegate
                        {
                            if (ComaPercent < 1f)
                            {
                                Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("WarningWakingInterruptsComa".Translate(pawn.Named("PAWN"), MinComaTicks.ToStringTicksToPeriod().Named("MINDURATION"), comaRestTicks.ToStringTicksToPeriod().Named("CURDURATION")), Wake, true);
                                Find.WindowStack.Add(window);
                            }
                            else
                            {
                                Wake();
                            }
                        }
                    };
                    yield return command_Action;
                    if (ComaPercent < 1f)
                    {
                        yield return new Command_Toggle
                        {
                            defaultLabel = "AutoWake".Translate().CapitalizeFirst(),
                            defaultDesc = "AutoWakeDesc".Translate(pawn.Named("PAWN")).Resolve(),
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
                            adjustedComaTicks = 240001f;
                            foreach (CompComaGeneBindable boundComp in BoundComps)
                            {
                                boundComp.presenceTicks = comaRestTicks;
                            }
                            Wake();
                        }
                    };
                }
            }
            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }
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
                            OffsetCapacity(newCap - comaRestCapacity, sendNotification: false);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            };
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "ComaCapacity".Translate().CapitalizeFirst(), comaRestCapacity.ToString(), "ComaCapacityDesc".Translate(), 1010);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref comaRestCapacity, "comaRestCapacity", 1);
            Scribe_Values.Look(ref adjustedComaTicks, "adjustedComaTicks", 0f);
            Scribe_Values.Look(ref comaRestTicks, "comaRestTicks", 0);
            Scribe_Values.Look(ref autoWake, "autoWake", false);
            Scribe_Values.Look(ref chamberThoughtIndex, "chamberThoughtIndex", -1);
            Scribe_Values.Look(ref notifiedWakeOK, "notifiedWakeOK", false);
            Scribe_Collections.Look(ref boundBuildings, "boundBuildings", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                boundBuildings.RemoveAll((Thing x) => x == null);
        }
    }
}

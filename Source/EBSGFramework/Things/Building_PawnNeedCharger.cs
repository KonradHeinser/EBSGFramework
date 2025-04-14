using Verse;
using RimWorld;
using Verse.Sound;
using UnityEngine;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class Building_PawnNeedCharger : Building
    {
        public GenDraw.FillableBarRequest BarDrawData => def.building.BarDrawDataFor(Rotation);

        private Pawn currentPawn;

        private float wasteProduced;

        private int wireExtensionTicks = 70;

        private Mote moteCharging;

        private Mote moteCablePulse;

        private Sustainer sustainerCharging;

        private EBSGExtension cachedExtension;

        private CompWasteProducer cachedWasteProducer;
        private bool checkedWaste;

        private CompThingContainer cachedContainer;
        private bool checkedContainer;

        public EBSGExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    cachedExtension = def.GetModExtension<EBSGExtension>();
                }

                return cachedExtension;
            }
        }

        public CompWasteProducer WasteProducer
        {
            get
            {
                if (cachedWasteProducer == null && !checkedWaste)
                {
                    cachedWasteProducer = GetComp<CompWasteProducer>();
                    checkedWaste = true;
                }
                return cachedWasteProducer;
            }
        }

        public CompThingContainer Container
        {
            get
            {
                if (cachedContainer == null && !checkedContainer)
                {
                    cachedContainer = GetComp<CompThingContainer>();
                    checkedContainer = true;
                }
                return cachedContainer;
            }
        }

        private Material wireMaterial;

        private Material WireMaterial
        {
            get
            {
                if (wireMaterial == null)
                {
                    wireMaterial = MaterialPool.MatFrom("Other/BundledWires", ShaderDatabase.Transparent, Color.white);
                }
                return wireMaterial;
            }
        }

        public bool CurrentlyInUse => currentPawn != null;

        public CompPowerTrader Power => this.TryGetComp<CompPowerTrader>();

        public CompRefuelable Fuel => this.TryGetComp<CompRefuelable>();

        private int WasteProducedPerChargingCycle => Container.Props.stackLimit;

        public bool IsPowered => Power == null || Power.PowerOn;

        public bool IsFueled => Fuel == null || Fuel.HasFuel;

        private bool rechargesFood;
        private bool alreadyCheckedFood;

        public bool RechargesFood
        {
            get
            {
                if (!alreadyCheckedFood)
                {
                    if (!Extension.needOffsetsPerHour.NullOrEmpty())
                        foreach (NeedOffset need in Extension.needOffsetsPerHour)
                        {
                            if (need.need == null) continue;
                            if (need.need == NeedDefOf.Food && need.offset > 0)
                            {
                                rechargesFood = true;
                                break;
                            }
                        }
                    alreadyCheckedFood = true;
                }


                return rechargesFood;
            }
        }

        private bool rechargesRest;
        private bool alreadyCheckedRest;

        public bool RechargesRest
        {
            get
            {
                if (!alreadyCheckedRest)
                {
                    if (!Extension.needOffsetsPerHour.NullOrEmpty())
                        foreach (NeedOffset need in Extension.needOffsetsPerHour)
                        {
                            if (need.need == null) continue;
                            if (need.need == NeedDefOf.Rest && need.offset > 0)
                            {
                                rechargesRest = true;
                                break;
                            }
                        }
                    alreadyCheckedRest = true;
                }

                return rechargesRest;
            }
        }

        public bool IsFullOfWaste
        {
            get
            {
                if (WasteProducer == null) return false;
                if (Container == null) return true;

                if (wasteProduced >= WasteProducedPerChargingCycle)
                {
                    return Container.innerContainer.Any;
                }
                return false;
            }
        }

        public bool PawnCanUse(Pawn pawn)
        {
            if (!IsPowered || IsFullOfWaste || !IsFueled) return false;
            if (!PawnHasUseForThis(pawn)) return false;
            if (!CurrentlyInUse || currentPawn == pawn) return true;

            return false;
        }

        public bool PawnHasUseForThis(Pawn pawn)
        {
            if (!Extension.needOffsetsPerHour.NullOrEmpty() && pawn.needs != null && !pawn.needs.AllNeeds.NullOrEmpty())
                foreach (NeedOffset needOffset in Extension.needOffsetsPerHour)
                    if (needOffset.offset > 0 && (needOffset.need == null || pawn.needs.TryGetNeed(needOffset.need) != null))
                        return true;
                    else if (!Extension.negativeNeedOffsetsAreNotCosts && needOffset.offset < 0 && pawn.needs.TryGetNeed(needOffset.need) == null)
                        return false; // If the offset is negative, then treat it as a required cost for use

            if (!Extension.resourceOffsetsPerHour.NullOrEmpty() && pawn.genes != null && !pawn.genes.GenesListForReading.NullOrEmpty())
                foreach (GeneEffect geneEffect in Extension.resourceOffsetsPerHour)
                    if (geneEffect.offset > 0 && pawn.HasRelatedGene(geneEffect.gene))
                        return true;
                    else if (!Extension.negativeResourceOffsetsAreNotCosts && geneEffect.offset < 0 && !pawn.HasRelatedGene(geneEffect.gene))
                        return false; // If the offset is negative, then treat it as a required cost for use

            return false;
        }

        public void StartCharging(Pawn pawn)
        {
            if (ModLister.CheckBiotech("Mech charging"))
            {
                if (currentPawn != null)
                {
                    Log.Error("Tried charging on already charging mech charger!");
                    return;
                }
                currentPawn = pawn;
                wireExtensionTicks = 0;
                Extension?.startSound?.PlayOneShot(this);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (currentPawn != null && (currentPawn.CurJobDef != EBSGDefOf.EBSG_NeedCharge || currentPawn.CurJob.targetA.Thing != this))
                StopCharging();
            if (currentPawn != null && IsPowered && IsFueled)
            {
                currentPawn.HandleNeedOffsets(Extension.needOffsetsPerHour, true, 1, true);
                currentPawn.HandleDRGOffsets(Extension.resourceOffsetsPerHour, 1, true);
                if (WasteProducer != null && Extension?.wastePerHourOfUse > 0)
                {
                    wasteProduced += Extension.wastePerHourOfUse * 0.0004f;
                    wasteProduced = Mathf.Clamp(wasteProduced, 0f, WasteProducedPerChargingCycle);
                    if (wasteProduced >= (float)WasteProducedPerChargingCycle && !Container.innerContainer.Any)
                    {
                        wasteProduced = 0f;
                        GenerateWastePack();
                    }
                }
                if ((moteCablePulse == null || moteCablePulse.Destroyed) && Extension?.chargeMotePulse != null)
                    moteCablePulse = MoteMaker.MakeInteractionOverlay(Extension.chargeMotePulse, this, new TargetInfo(InteractionCell, base.Map));
                moteCablePulse?.Maintain();
            }
            if (currentPawn != null && IsPowered && IsFueled && CurrentlyInUse)
            {
                if (sustainerCharging == null && Extension?.sustainerSound != null)
                    sustainerCharging = Extension.sustainerSound.TrySpawnSustainer(SoundInfo.InMap(this));
                sustainerCharging?.Maintain();
                if ((moteCharging == null || moteCharging.Destroyed) && Extension?.chargeMote != null)
                    moteCharging = MoteMaker.MakeAttachedOverlay(currentPawn, ThingDefOf.Mote_MechCharging, Vector3.zero);
                moteCharging?.Maintain();
            }
            else if (sustainerCharging != null && (currentPawn == null || !IsPowered || !IsFueled))
            {
                sustainerCharging.End();
                sustainerCharging = null;
            }
            if (wireExtensionTicks < 70)
                wireExtensionTicks++;
        }

        public bool PawnShouldStop(Pawn pawn)
        {
            // If the pawn is low on food or sleep and this does not directly impact those, stop to avoid bad things from happening.
            if (pawn.needs != null && pawn.needs.AllNeeds.NullOrEmpty())
                if ((!RechargesFood && pawn.needs.TryGetNeed(NeedDefOf.Food)?.CurLevelPercentage < 0.20) ||
                    (!RechargesRest && pawn.needs.TryGetNeed(NeedDefOf.Rest)?.CurLevelPercentage < 0.20)) return true;

            bool flag = true; // If none of the recharging needs/resources are below the 95% threshold, this will cause the pawn to automatically continue
            if (!Extension.needOffsetsPerHour.NullOrEmpty() && pawn.needs != null && !pawn.needs.AllNeeds.NullOrEmpty())
                foreach (NeedOffset need in Extension.needOffsetsPerHour)
                {
                    if (need.need == null) continue; // If it's random, don't worry about it because it wouldn't cause automatic actiation, and it's on the player at that point if all are random.
                    if (flag && need.offset > 0 && pawn.needs.TryGetNeed(need.need) != null && pawn.needs.TryGetNeed(need.need).CurLevelPercentage < 0.95)
                    {
                        flag = false; // If the need is being recharged and the current level percentage is below 95%, keep recharging
                        break;
                    }

                    if (need.offset < 0 && pawn.needs.TryGetNeed(need.need) != null && pawn.needs.TryGetNeed(need.need).CurLevelPercentage < 0.20)
                        return true; // In the event that a specific need is being drained and it is low, stop
                }

            if (!Extension.resourceOffsetsPerHour.NullOrEmpty() && pawn.genes != null && !pawn.genes.GenesListForReading.NullOrEmpty())
                foreach (GeneEffect geneEffect in Extension.resourceOffsetsPerHour)
                {
                    if (!pawn.HasRelatedGene(geneEffect.gene))
                    {
                        if (geneEffect.offset > 0) continue;
                        if (!Extension.negativeResourceOffsetsAreNotCosts) return true;
                    }

                    if (pawn.genes.GetGene(geneEffect.gene) is ResourceGene resourceGene)
                    {
                        if (flag && geneEffect.offset > 0 && resourceGene.ValuePercent < 0.95)
                        {
                            flag = false; // If the resource is being recharged and the current level percentage is below 95%, keep recharging
                            break;
                        }
                        if (geneEffect.offset < 0 && resourceGene.ValuePercent < 0.20)
                            return true; // In the event that a specific resource is being drained and it is low, stop
                    }
                }

            return flag;
        }

        public void GenerateWastePack()
        {
            if (WasteProducer == null) return;
            WasteProducer.ProduceWaste(WasteProducedPerChargingCycle);
            if (Extension?.wasteProducedEffecter != null)
                Extension.wasteProducedEffecter.Spawn(base.Position, base.Map).Cleanup();
        }

        public void StopCharging()
        {
            if (currentPawn == null) return;
            currentPawn = null;
            wireExtensionTicks = 0;
        }

        public override void PostPostMake()
        {
            if (Extension == null)
            {
                Log.Error(def + " is missing the EBSGExtension, which is required for the Building_PawnNeedCharger class to function. Destroying building to avoid more errors.");
                Destroy();
            }
            else base.PostPostMake();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            WasteProducer?.ProduceWaste(Mathf.CeilToInt(wasteProduced));
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref currentPawn, "currentPawn");
            Scribe_Values.Look(ref wasteProduced, "wasteProduced", 0f);
            Scribe_Values.Look(ref wireExtensionTicks, "wireExtensionTicks", 0);
        }
    }
}

using System.Collections.Generic;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class Building_PawnNeedCharger : Building
    {
        private Pawn currentPawn;

        private float wasteProduced;

        private int wireExtensionTicks = 70;

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

        public bool CurrentlyInUse => currentPawn != null;

        public CompPowerTrader Power => this.TryGetComp<CompPowerTrader>();

        private int WasteProducedPerChargingCycle => Container.Props.stackLimit;

        public bool IsPowered => Power == null || Power.PowerOn;

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
            if (!IsPowered || IsFullOfWaste) return false;

            if (!CurrentlyInUse || currentPawn == pawn) return true;

            return false;
        }

        public bool PawnNeeds(Pawn pawn)
        {
            if (pawn.needs == null || pawn.needs.AllNeeds.NullOrEmpty()) return false;

            if (!Extension.needOffsets.NullOrEmpty())
                foreach (NeedOffset needOffset in Extension.needOffsets)
                    if (needOffset.need == null || pawn.needs.TryGetNeed(needOffset.need) != null)
                        return true;

            return false;
        }

        public override void PostPostMake()
        {
            if (Extension == null)
            {
                Log.Error(def + " is missing the EBSGExtension, which is required for the Building_PawnNeedCharger class to work. Destroying building to avoid more errors.");
                Destroy();
            }
            else base.PostPostMake();

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

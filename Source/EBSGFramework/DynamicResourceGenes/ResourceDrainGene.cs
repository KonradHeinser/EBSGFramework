using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ResourceDrainGene : HediffAdder, IGeneResourceDrain
    {
        [Unsaved(false)]
        private ResourceGene cachedResourceGene;

        DRGExtension DRGextension = null;

        public DRGExtension DRGExtension
        {
            get
            {
                if (!extensionAlreadyChecked)
                {
                    DRGextension = def.GetModExtension<DRGExtension>();
                    extensionAlreadyChecked = true;
                }
                return DRGextension;
            }
        }

        public bool extensionAlreadyChecked = false;

        public bool CanOffset
        {
            get
            {
                if (Active && !pawn.Dead)
                {
                    if (DRGExtension != null)
                    {
                        if (pawn.Spawned)
                        {
                            float time = GenLocalDate.DayPercent(Pawn);
                            if (time < DRGExtension.startTime || time > DRGExtension.endTime) return false;

                            if (pawn.Map != null)
                            {
                                float light = pawn.Map.glowGrid.GroundGlowAt(pawn.Position);
                                if (light < DRGExtension.minLightLevel || light > DRGExtension.maxLightLevel) return false;
                            }
                        }

                        if (!DRGExtension.requireOneOfHediffs.NullOrEmpty() && !pawn.PawnHasAnyOfHediffs(DRGExtension.requireOneOfHediffs)) return false;
                        if (!pawn.PawnHasAllOfHediffs(DRGExtension.requiredHediffs)) return false;
                        if (pawn.PawnHasAnyOfHediffs(DRGExtension.forbiddenHediffs)) return false;

                        if (!pawn.AllNeedLevelsMet(DRGExtension.needLevels)) return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public Gene_Resource Resource
        {
            get
            {
                if (!def.HasModExtension<DRGExtension>()) Log.Error(def + "doesn't have the DRG extension, meaning the main resource gene cannot be found.");
                else if (cachedResourceGene == null || !cachedResourceGene.Active)
                {
                    cachedResourceGene = (ResourceGene)pawn.genes.GetGene(def.GetModExtension<DRGExtension>().mainResourceGene);
                }
                return cachedResourceGene;
            }
        }

        public float ResourceLossPerDay => GetLossPerDay();

        public Pawn Pawn => pawn;

        public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

        private float GetLossPerDay()
        {
            if (def.GetModExtension<DRGExtension>() == null) return def.resourceLossPerDay;
            if (def.GetModExtension<DRGExtension>().passiveFactorStat != null) return def.resourceLossPerDay * pawn.GetStatValue(def.GetModExtension<DRGExtension>().passiveFactorStat);
            return def.resourceLossPerDay;
        }

        public override void Tick()
        {
            base.Tick();
            if (Resource != null) ResourceGene.OffsetResource(pawn, ResourceLossPerDay * -1, cachedResourceGene, DRGExtension, false, true, true);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }
    }
}

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ResourceDrainGene : Gene, IGeneResourceDrain
    {
        [Unsaved(false)]
        private ResourceGene cachedResourceGene;

        DRGExtension extension = null;

        public List<AbilityDef> addedAbilities;

        public int cachedGeneCount = 0;

        public bool extensionAlreadyChecked = false;

        public bool CanOffset
        {
            get
            {
                if (Active && !pawn.Dead)
                {
                    if (extension == null && !extensionAlreadyChecked)
                    {
                        extension = def.GetModExtension<DRGExtension>();
                        extensionAlreadyChecked = true;
                    }
                    if (extension != null)
                    {
                        if (pawn.Spawned)
                        {
                            float time = GenLocalDate.DayPercent(Pawn);
                            if (time < extension.startTime || time > extension.endTime) return false;

                            if (pawn.Map != null)
                            {
                                float light = pawn.Map.glowGrid.GroundGlowAt(pawn.Position);
                                if (light < extension.minLightLevel || light > extension.maxLightLevel) return false;
                            }
                        }

                        if (!extension.requireOneOfHediffs.NullOrEmpty() && !EBSGUtilities.PawnHasAnyOfHediffs(pawn, extension.requireOneOfHediffs)) return false;
                        if (!EBSGUtilities.PawnHasAllOfHediffs(pawn, extension.requiredHediffs)) return false;
                        if (EBSGUtilities.PawnHasAnyOfHediffs(pawn, extension.forbiddenHediffs)) return false;

                        if (!EBSGUtilities.AllNeedLevelsMet(pawn, extension.needLevels)) return false;
                    }
                    return true;
                }
                return false;
            }
        }


        private const float MinAgeForDrain = 3f;

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

            if (pawn.IsHashIntervalTick(200))
            {
                EBSGExtension EBSGextension = def.GetModExtension<EBSGExtension>();
                if (EBSGextension != null && !EBSGextension.geneAbilities.NullOrEmpty() && pawn.genes.GenesListForReading.Count != cachedGeneCount)
                {
                    if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                    addedAbilities = SpawnAgeLimiter.AbilitiesWithCertainGenes(pawn, EBSGextension.geneAbilities, addedAbilities);
                    cachedGeneCount = pawn.genes.GenesListForReading.Count;
                }
            }

            if (extension == null && !extensionAlreadyChecked)
            {
                extension = def.GetModExtension<DRGExtension>();
                extensionAlreadyChecked = true;
            }
            if (Resource != null) ResourceGene.OffsetResource(pawn, ResourceLossPerDay * -1, cachedResourceGene, extension, false, true, true);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            EBSGExtension EBSGextension = def.GetModExtension<EBSGExtension>();
            HediffAdder.HediffAdding(pawn, this);
            if (EBSGextension != null)
            {
                if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                SpawnAgeLimiter.LimitAge(pawn, EBSGextension.expectedAges, EBSGextension.ageRange, EBSGextension.sameBioAndChrono);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            HediffAdder.HediffRemoving(pawn, this);

            if (!addedAbilities.NullOrEmpty())
            {
                EBSGUtilities.RemovePawnAbilities(pawn, addedAbilities);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref addedAbilities, "EBSG_DRGDAddedAbilities");
        }
    }
}

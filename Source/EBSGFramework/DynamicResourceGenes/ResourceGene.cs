using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class ResourceGene : Gene_Resource, IGeneResourceDrain
    {
        public bool resourcePacksAllowed = true;

        public Gene_Resource Resource => this;

        DRGExtension extension = null;

        private EBSGExtension EBSGextension = null;

        private bool checkedEBSGExtension;

        public EBSGExtension EBSGExtension
        {
            get
            {
                if (!checkedEBSGExtension)
                {
                    EBSGextension = def.GetModExtension<EBSGExtension>();
                    checkedEBSGExtension = true;
                }
                return EBSGextension;
            }
        }

        public int cachedGeneCount = 0;

        public List<AbilityDef> addedAbilities;

        public bool extensionAlreadyChecked = false;

        public Pawn Pawn => pawn;

        public StatDef statFactor => extension?.gainStat;

        [Unsaved(false)]
        private List<IGeneResourceDrain> tmpDrainGenes = new List<IGeneResourceDrain>();

        public float AmountMissing => Max - Value;

        public bool CanOffset
        {
            get
            {
                if (Active && !pawn.Dead)
                {
                    if (extension == null && !extensionAlreadyChecked) InitializeExtension();
                    if (extension != null)
                    {
                        if (pawn.Spawned)
                        {
                            float time = GenLocalDate.DayPercent(Pawn);
                            if (extension.progressThroughDay.ValidValue(time) != extension.invertTime)
                                return false;

                            if (pawn.Map != null)
                            {
                                float light = pawn.Map.glowGrid.GroundGlowAt(pawn.Position);
                                if (extension.lightLevel.ValidValue(light) != extension.invertLight)
                                    return false;
                            }
                        }

                        if (!extension.requireOneOfHediffs.NullOrEmpty() && !pawn.PawnHasAnyOfHediffs(extension.requireOneOfHediffs)) return false;
                        if (!pawn.PawnHasAllOfHediffs(extension.requiredHediffs)) return false;
                        if (pawn.PawnHasAnyOfHediffs(extension.forbiddenHediffs)) return false;

                        if (!pawn.AllNeedLevelsMet(extension.needLevels)) return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

        public float ResourceLossPerDay => GetLossPerDay();

        public override float InitialResourceMax => 1f;

        public override float MinLevelForAlert => 0.15f;

        public override float MaxLevelOffset => 0.1f;

        public float overchargeLeft;

        public float underchargeLeft;

        protected override Color BarColor => GetColor();

        protected override Color BarHighlightColor => GetHighlightColor();

        private float GetLossPerDay()
        {
            if (extension == null) InitializeExtension();
            if (extension.passiveFactorStat != null) 
                return def.resourceLossPerDay * pawn.GetStatValue(extension.passiveFactorStat);
            return def.resourceLossPerDay;
        }

        public Color GetColor()
        {
            if (def.HasModExtension<DRGExtension>()) 
                return def.GetModExtension<DRGExtension>().barColor;
            return new ColorInt(138, 3, 3).ToColor;
        }

        public Color GetHighlightColor()
        {
            if (def.HasModExtension<DRGExtension>()) 
                return def.GetModExtension<DRGExtension>().barHighlightColor;
            return new ColorInt(145, 42, 42).ToColor;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (extension == null) InitializeExtension();
            Reset();
            HediffAdder.HediffAdding(pawn, this);
            if (EBSGExtension != null)
            {
                if (addedAbilities == null) 
                    addedAbilities = new List<AbilityDef>();
                SpawnAgeLimiter.GetGender(pawn, EBSGExtension, def);
                SpawnAgeLimiter.LimitAge(pawn, EBSGExtension.expectedAges, EBSGExtension.ageRange, EBSGextension.sameBioAndChrono);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            HediffAdder.HediffRemoving(pawn, this);

            if (!addedAbilities.NullOrEmpty())
                pawn.RemovePawnAbilities(addedAbilities);
        }

        public List<IGeneResourceDrain> DrainGenes
        {
            get
            {
                tmpDrainGenes.Clear();
                List<Gene> genesListForReading = pawn.genes.GenesListForReading;
                for (int i = 0; i < genesListForReading.Count; i++)
                    if (genesListForReading[i] is IGeneResourceDrain geneResourceDrain && genesListForReading[i].def.HasModExtension<DRGExtension>() && genesListForReading[i].def.GetModExtension<DRGExtension>().mainResourceGene == def)
                        tmpDrainGenes.Add(geneResourceDrain);

                return tmpDrainGenes;
            }
        }

        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            if (extension == null || !extension.checkIngestion) return;
            IngestibleProperties ingestible = thing.def.ingestible;
            if (ingestible == null) return;
            if (thing.def.IsEgg) OffsetResource(pawn, extension.eggIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
            if (thing.def.IsDrug) OffsetResource(pawn, extension.drugIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
            if (thing.def.IsCorpse) OffsetResource(pawn, extension.corpseIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
            if (thing.def.IsMeat)
            {
                if (ingestible.sourceDef?.race?.Humanlike == true)
                    OffsetResource(pawn, extension.humanlikeIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
                else
                    OffsetResource(pawn, extension.genericMeatIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
            }
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (extension == null && !extensionAlreadyChecked)
                InitializeExtension();

            if (extension != null && (extension.maxStat != null || extension.maxFactorStat != null))
                CreateMax(extension.maximum, extension.maxStat, extension.maxFactorStat);

            OffsetResource(pawn, ResourceLossPerDay * -1 * delta, this, extension, false, true, true);
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(200) && EBSGExtension?.geneAbilities.NullOrEmpty() == false && 
                pawn.genes.GenesListForReading.Count != cachedGeneCount)
            {
                if (addedAbilities == null) 
                    addedAbilities = new List<AbilityDef>();

                addedAbilities = SpawnAgeLimiter.AbilitiesWithCertainGenes(pawn, EBSGExtension.geneAbilities, addedAbilities);
                cachedGeneCount = pawn.genes.GenesListForReading.Count;
            }

            if (pawn.IsHashIntervalTick(2500) && EBSGExtension?.genderByAge.NullOrEmpty() == false && 
                (EBSGExtension.genderByAge.Count > 1 || EBSGextension.genderByAge[0].range != GenderByAge.defaultRange))
                SpawnAgeLimiter.GetGender(pawn, EBSGExtension, def);
        }
        
        public void CreateMax(float maximum = 1f, StatDef maxStat = null, StatDef maxFactorStat = null)
        {
            float newMax;
            if (maxStat != null) newMax = pawn.GetStatValue(maxStat);
            else newMax = maximum;
            if (maxFactorStat != null) newMax *= pawn.GetStatValue(maxFactorStat);
            SetMax(newMax);
        }

        public override void SetTargetValuePct(float val)
        {
            targetValue = Mathf.Clamp(val * Max, 0f, Max - MaxLevelOffset);
        }

        public bool ShouldConsumeResourceNow()
        {
            return Value < targetValue;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }

        public void InitializeExtension()
        {
            extension = def.GetModExtension<DRGExtension>();
            extensionAlreadyChecked = true;
            if (extension != null)
            {
                resourcePacksAllowed = extension.resourcePacksAllowed;
                if (extension.maximum != 1f || extension.maxStat != null || extension.maxFactorStat != null) CreateMax(extension.maximum, extension.maxStat, extension.maxFactorStat);
            }
            else Log.Error(def + "is missing the DRGExtension modex");
        }

        public static void OffsetResource(Pawn pawn, float offset, ResourceGene resourceGene, DRGExtension extension = null, bool applyGainStat = false, bool dailyValue = false, bool checkPassiveStat = false, bool storeLimitPassing = false)
        {
            if (resourceGene == null) return;
            if (extension == null)
                extension = resourceGene.def.GetModExtension<DRGExtension>();
            if (offset > 0f && applyGainStat && extension.gainStat != null)
                offset *= pawn.GetStatValue(extension.gainStat);

            if (dailyValue) offset /= 60000f;
            if (checkPassiveStat && extension.passiveFactorStat != null)
                offset *= pawn.GetStatValue(extension.passiveFactorStat);
            if (resourceGene != null)
            {
                if (resourceGene.overchargeLeft > 0)
                {
                    resourceGene.overchargeLeft += offset;
                    if (resourceGene.overchargeLeft < 0)
                    {
                        resourceGene.Value += resourceGene.overchargeLeft;
                        resourceGene.overchargeLeft = 0;
                    }
                }
                else if (resourceGene.underchargeLeft > 0)
                {
                    resourceGene.underchargeLeft += offset;
                    if (resourceGene.underchargeLeft < 0)
                    {
                        resourceGene.Value += resourceGene.underchargeLeft * -1; // Need the value to rise by whatever remains
                        resourceGene.underchargeLeft = 0;
                    }
                }
                else
                {
                    if (storeLimitPassing)
                        if (resourceGene.Value + offset > resourceGene.Max)
                            resourceGene.overchargeLeft += resourceGene.Value + offset - resourceGene.Max;
                        else if (resourceGene.Value + offset < 0)
                            resourceGene.underchargeLeft += resourceGene.Value + offset * -1;
                    resourceGene.Value += offset;
                }

                if (resourceGene.Value <= 0f && extension.cravingHediff != null && !pawn.health.hediffSet.HasHediff(extension.cravingHediff))
                    pawn.health.AddHediff(extension.cravingHediff);
                if (resourceGene.Value >= resourceGene.max && extension.overchargeHediff != null && !pawn.health.hediffSet.HasHediff(extension.overchargeHediff))
                    pawn.health.AddHediff(extension.overchargeHediff);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref resourcePacksAllowed, "resourcePacksAllowed", true);
            Scribe_Values.Look(ref overchargeLeft, "overchargeLeft", 0f);
            Scribe_Values.Look(ref underchargeLeft, "underchargeLeft", 0f);
            Scribe_Collections.Look(ref addedAbilities, "EBSG_DRGAddedAbilities");
        }
    }
}

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

        DRGExtension extension;

        private EBSGExtension EBSGextension;

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

        public int cachedGeneCount;

        public List<AbilityDef> addedAbilities;

        public bool extensionAlreadyChecked;

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
                        float time = GenLocalDate.DayPercent(Pawn);
                        if (extension.progressThroughDay.ValidValue(time) != extension.invertTime)
                            return false;

                        int doq = GenLocalDate.DayOfQuadrum(pawn);
                        if (extension.daysOfQuadrum.ValidValue(doq) == extension.invertDOQ)
                            return false;

                        int doy = GenLocalDate.DayOfYear(pawn);
                        if (extension.daysOfYear.ValidValue(doy) == extension.invertDOY)
                            return false;

                        if (!pawn.CheckSeason(extension.seasons, true))
                            return false;

                        if (pawn.Spawned)
                        {
                            float light = pawn.Map.glowGrid.GroundGlowAt(pawn.Position);
                            if (extension.lightLevel.ValidValue(light) == extension.invertLight)
                                return false;
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
            return def.HasModExtension<DRGExtension>() ? def.GetModExtension<DRGExtension>().barColor : new ColorInt(138, 3, 3).ToColor;
        }

        public Color GetHighlightColor()
        {
            return def.HasModExtension<DRGExtension>() ? def.GetModExtension<DRGExtension>().barHighlightColor : new ColorInt(145, 42, 42).ToColor;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            if (extension == null) 
                InitializeExtension();
            Reset();
            HediffAdder.HediffAdding(pawn, this);
            resourcePacksAllowed = extension?.resourcePacks.NullOrEmpty() == false;
            if (EBSGExtension != null)
            {
                if (addedAbilities == null) 
                    addedAbilities = new List<AbilityDef>();
                SpawnAgeLimiter.GetGender(pawn, EBSGExtension);
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
                foreach (var t in genesListForReading)
                    if (t is IGeneResourceDrain geneResourceDrain && t.def.HasModExtension<DRGExtension>() && t.def.GetModExtension<DRGExtension>().mainResourceGene == def)
                        tmpDrainGenes.Add(geneResourceDrain);

                return tmpDrainGenes;
            }
        }

        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            if (extension == null) return;
            if (thing.def.IsEgg && extension.eggIngestionEffect != 0f) 
                OffsetResource(pawn, extension.eggIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
            if (thing.def.IsDrug && extension.drugIngestionEffect != 0f)
                OffsetResource(pawn, extension.drugIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
            if (thing is Corpse corpse && extension.corpseIngestionEffect != 0f)
                if (corpse.InnerPawn?.RaceProps.Humanlike == true)
                {
                    if (extension.humanlikeCorpseIngestionEffect != 0f)
                        OffsetResource(pawn, extension.humanlikeCorpseIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
                }
                else if (extension.corpseIngestionEffect != 0f)
                    OffsetResource(pawn, extension.corpseIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
            
            if (thing.def.IsMeat && thing.def.ingestible != null)
            {
                if (thing.def.ingestible.sourceDef?.race?.Humanlike == true)
                {
                    if (extension.humanlikeIngestionEffect != 0f)
                        OffsetResource(pawn, extension.humanlikeIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
                }
                else if (extension.genericMeatIngestionEffect != 0f)
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
                SpawnAgeLimiter.GetGender(pawn, EBSGExtension);
        }
        
        public void CreateMax(float maximum = 1f, StatDef maxStat = null, StatDef maxFactorStat = null)
        {
            var newMax = maxStat != null ? pawn.GetStatValue(maxStat) : maximum;
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
            foreach (var g in base.GetGizmos())
                yield return g;
            
            foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
                yield return resourceDrainGizmo;
        }

        public void InitializeExtension()
        {
            extension = def.GetModExtension<DRGExtension>();
            extensionAlreadyChecked = true;
            if (extension != null)
            {
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

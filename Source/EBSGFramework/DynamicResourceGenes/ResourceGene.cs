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

        public bool extensionAlreadyChecked = false;

        public Pawn Pawn => pawn;

        [Unsaved(false)]
        private List<IGeneResourceDrain> tmpDrainGenes = new List<IGeneResourceDrain>();

        public bool CanOffset
        {
            get
            {
                if (Active)
                {
                    return true;
                }
                return false;
            }
        }

        public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

        public float ResourceLossPerDay => def.resourceLossPerDay;

        public override float InitialResourceMax => 1f;

        public override float MinLevelForAlert => 0.15f;

        public override float MaxLevelOffset => 0.1f;
        
        protected override Color BarColor => GetColor();

        protected override Color BarHighlightColor => GetHighlightColor();

        public Color GetColor()
        {
            if (def.HasModExtension<DRGExtension>()) return def.GetModExtension<DRGExtension>().barColor;
            return new ColorInt(138, 3, 3).ToColor;
        }

        public Color GetHighlightColor()
        {
            if (def.HasModExtension<DRGExtension>()) return def.GetModExtension<DRGExtension>().barHighlightColor;
            return new ColorInt(145, 42, 42).ToColor;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            InitializeExtension();
            Reset();
        }

        private List<IGeneResourceDrain> DrainGenes
        {
            get
            {
                tmpDrainGenes.Clear();
                List<Gene> genesListForReading = pawn.genes.GenesListForReading;
                for (int i = 0; i < genesListForReading.Count; i++)
                {
                    if (genesListForReading[i] is IGeneResourceDrain geneResourceDrain && genesListForReading[i].def.HasModExtension<DRGExtension>() && genesListForReading[i].def.GetModExtension<DRGExtension>().mainResourceGene == def)
                    {
                        tmpDrainGenes.Add(geneResourceDrain);
                    }
                }
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
                {
                    OffsetResource(pawn, extension.humanlikeIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
                }
                else
                {
                    OffsetResource(pawn, extension.genericMeatIngestionEffect * thing.GetStatValue(StatDefOf.Nutrition) * numTaken, this, extension);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (extension == null && !extensionAlreadyChecked)
            {
                InitializeExtension();
            }
            if (extension != null)
            {
                if (extension.maxStat != null || extension.maxFactorStat != null) CreateMax(extension.maximum, extension.maxStat, extension.maxFactorStat);
            }
            OffsetResource(pawn, ResourceLossPerDay * -1, this, extension, false, true);
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


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref resourcePacksAllowed, "resourcePacksAllowed", true);
        }

        public static void OffsetResource(Pawn pawn, float offset, ResourceGene resourceGene, DRGExtension extension = null, bool applyStatFactor = true, bool dailyValue = false)
        {
            if (offset > 0f && applyStatFactor && extension.gainStat != null)
            {
                offset *= pawn.GetStatValue(extension.gainStat);
            }
            if (dailyValue) offset /= 60000f;
            if (resourceGene != null)
            {
                resourceGene.Value += offset;
                if (resourceGene.Value <= 0f && extension.cravingHediff != null && !pawn.health.hediffSet.HasHediff(extension.cravingHediff))
                {
                    pawn.health.AddHediff(extension.cravingHediff);
                }
                if (resourceGene.Value >= resourceGene.max && extension.overchargeHediff != null && !pawn.health.hediffSet.HasHediff(extension.overchargeHediff))
                {
                    if (pawn.health.hediffSet.HasHediff(extension.overchargeHediff)) Log.Error("What the fuck");
                    pawn.health.AddHediff(extension.overchargeHediff);
                }
            }
        }

    }
}

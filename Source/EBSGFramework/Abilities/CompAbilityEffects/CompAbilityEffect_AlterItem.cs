using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_AlterItem : CompAbilityEffect
    {
        public new CompProperties_AbilityAlterItem Props => props as CompProperties_AbilityAlterItem;
        
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            
            var t = target.Thing;
            if (t == null || t is Pawn)
                return;

            t.HitPoints += Props.hpRestore + Mathf.CeilToInt(Props.maxHPPercentRestore * t.MaxHitPoints);
            if (Props.uncode && t.TryGetComp<CompBiocodable>(out var code) && code.Biocoded)
                code.UnCode();
            if (Props.untaint && t is Apparel apparel && apparel.WornByCorpse)
                apparel.WornByCorpse = false;

            if (t.TryGetComp<CompQuality>(out var compQuality))
            {
                if (Props.increaseQuality && compQuality.Quality < Props.maxQuality)
                    switch (compQuality.Quality)
                    {
                        case QualityCategory.Awful:
                            compQuality.SetQuality(QualityCategory.Poor, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Poor:
                            compQuality.SetQuality(QualityCategory.Normal, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Normal:
                            compQuality.SetQuality(QualityCategory.Good, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Good:
                            compQuality.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Excellent:
                            compQuality.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Masterwork:
                            compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Colony);
                            break;
                    }

                if (Props.decreaseQuality && compQuality.Quality > Props.minQuality)
                    switch (compQuality.Quality)
                    {
                        case QualityCategory.Poor:
                            compQuality.SetQuality(QualityCategory.Awful, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Normal:
                            compQuality.SetQuality(QualityCategory.Poor, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Good:
                            compQuality.SetQuality(QualityCategory.Normal, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Excellent:
                            compQuality.SetQuality(QualityCategory.Good, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Masterwork:
                            compQuality.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Colony);
                            break;
                        case QualityCategory.Legendary:
                            compQuality.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Colony);
                            break;
                    }
            }

            if (t.Stuff != null && Props.stuffingChange.Any())
            {
                var stuffChange = Props.stuffingChange.FirstOrDefault(li => li.Contains(t.Stuff));
                if (stuffChange != null)
                {
                    var flag = false;
                    foreach (var stuff in stuffChange)
                    {
                        if (stuff == t.Stuff)
                            flag = true;
                        else if (flag && t.def.stuffCategories.ContainsAny(s => stuff.stuffProps?.categories?.Contains(s) == true))
                        {
                            t.SetStuffDirect(stuff);
                            break;
                        }
                    }
                }
            }
        }
    }
}

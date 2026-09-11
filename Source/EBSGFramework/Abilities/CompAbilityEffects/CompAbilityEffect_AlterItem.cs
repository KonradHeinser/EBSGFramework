using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_AlterItem : CompAbilityEffect
    {
        public new CompProperties_AbilityAlterItem Props => props as CompProperties_AbilityAlterItem;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (Props.requireTaint || Props.requireBiocode || Props.requireQuality || Props.requireAny || Props.requireHitPoints || Props.requireStuffing)
            {   
                var t = target.Thing;
                if (t == null)
                    return false;
                
                if (Props.requireFinished && t is UnfinishedThing)
                    return false;

                var flag = Props.requireAny; // This will become false in the event that any of the things the comp will try to do during Apply can happen
                
                if (Props.requireHitPoints && !t.def.useHitPoints)
                    return false;
                
                if (flag && t.def.useHitPoints && (Props.hpRestore != 0 || Props.maxHPPercentRestore != 0))
                    flag = false;

                if (Props.requireStuffing && (t.Stuff == null || !CanReforge(t.Stuff)))
                    return false;
                
                if (flag && t.Stuff != null && CanReforge(t.Stuff))
                    flag = false;
                
                if (Props.requireQuality && (!t.TryGetComp<CompQuality>(out var quality) || 
                    (Props.increaseQuality && quality.Quality >= Props.maxQuality) || (Props.decreaseQuality && quality.Quality <= Props.minQuality)))
                    return false;
                
                if (flag && t.TryGetComp<CompQuality>(out var q) &&
                    ((Props.increaseQuality && q.Quality < Props.maxQuality) || (Props.decreaseQuality && q.Quality > Props.minQuality)))
                    flag = false;

                if (Props.requireBiocode && t.TryGetComp<CompBiocodable>()?.Biocoded != true)
                    return false;
                
                if (flag && Props.uncode && t.TryGetComp<CompBiocodable>()?.Biocoded == true)
                    flag = false;
                
                if (t is Apparel apparel)
                {
                    if (Props.requireTaint && !apparel.WornByCorpse)
                        return false;
                    
                    if (flag && Props.untaint && apparel.WornByCorpse)
                        flag = false;
                }
                else if (Props.requireTaint)
                    return false;

                if (flag)
                    return false;
                
                bool CanReforge(ThingDef stuff)
                {
                    var change = Props.stuffingChange.FirstOrDefault(li => li.Contains(stuff));
                    if (change == null) return false;
                    var flag2 = false;
                    foreach (var st in change)
                    {
                        if (st == t.Stuff)
                            flag2 = true;
                        else if (flag2 && t.def.stuffCategories.ContainsAny(s => st.stuffProps?.categories?.Contains(s) == true))
                            return true;
                    }
                    return false;
                }
            }
            
            return base.Valid(target, throwMessages);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            
            var t = target.Thing;
            if (t == null || t is Pawn)
                return;

            t.HitPoints = Mathf.Clamp(t.HitPoints + Props.hpRestore + Mathf.CeilToInt(Props.maxHPPercentRestore * t.MaxHitPoints), 0, t.MaxHitPoints);
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
                            var hpPer = t.HitPoints / t.MaxHitPoints;
                            t.SetStuffDirect(stuff);
                            StatDefOf.MaxHitPoints.Worker.ClearCacheForThing(t);
                            t.Notify_ColorChanged();
                            t.DirtyMapMesh(t.MapHeld);
                            if (t.HitPoints / t.MaxHitPoints != hpPer)
                                t.HitPoints = hpPer * t.MaxHitPoints;
                            if (t is UnfinishedThing unfinishedThing)
                            {
                                int num2 = unfinishedThing.ingredients.Sum(ingredient => ingredient.stackCount);
                                int num3 = 100;
                                unfinishedThing.ingredients.Clear();
                                while (num2 > 0 && num3-- > 0)
                                {
                                    Thing thing = ThingMaker.MakeThing(stuff);
                                    thing.stackCount = Mathf.Min(num2, stuff.stackLimit);
                                    unfinishedThing.ingredients.Add(thing);
                                    num2 -= thing.stackCount;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}

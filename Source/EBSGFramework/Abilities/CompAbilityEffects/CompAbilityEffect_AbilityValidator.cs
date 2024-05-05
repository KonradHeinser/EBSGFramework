using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_AbilityValidator : CompAbilityEffect
    {
        public new CompProperties_AbilityAbilityValidator Props => (CompProperties_AbilityAbilityValidator)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages)) return false;

            string baseExplanation = "CannotUseAbility".Translate(parent.def.label) + ": ";

            // Caster checks
            if (!CheckCasterLight(out string casterLightExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + casterLightExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return true;
            }
            if (!CheckCasterHediffs(out string casterHediffExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + casterHediffExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckCasterPawn(out string casterExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + casterExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return true;
            }

            // Caster map checks
            if (!CheckRain(out string rainExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + rainExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return true;
            }
            if (!CheckSnow(out string snowExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + snowExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return true;
            }
            if (!CheckCondition(out string conditionExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + conditionExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckWeather(out string weatherExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + weatherExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckHour(out string hourExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + hourExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Target checks
            if (!CheckTargetLight(target, out string targetLightExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + targetLightExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckTargetHediffs(target, out string targetHediffExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + targetHediffExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckTargetGenes(target, out string targetGeneExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + targetGeneExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckTargetPawn(target, out string targetExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + targetExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return true;
            }

            return true;
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (Props.disableGizmo)
            {
                if (!CheckRain(out string rainExplanation))
                {
                    reason = rainExplanation;
                    return true;
                }
                if (!CheckSnow(out string snowExplanation))
                {
                    reason = snowExplanation;
                    return true;
                }
                if (!CheckCasterLight(out string casterLightExplanation))
                {
                    reason = casterLightExplanation;
                    return true;
                }
                if (!CheckCasterHediffs(out string casterHediffExplanation))
                {
                    reason = casterHediffExplanation;
                    return true;
                }
                if (!CheckCondition(out string conditionExplanation))
                {
                    reason = conditionExplanation;
                    return true;
                }
                if (!CheckWeather(out string weatherExplanation))
                {
                    reason = weatherExplanation;
                    return true;
                }
                if (!CheckHour(out string hourExplanation))
                {
                    reason = hourExplanation;
                    return true;
                }
                if (!CheckCasterPawn(out string casterExplanation))
                {
                    reason = casterExplanation;
                    return true;
                }
            }
            reason = null;
            return false;
        }

        public bool CheckRain(out string explanation)
        {
            Map map = parent.pawn.Map;
            if (map != null)
            {
                if (Props.minimumRainRate > map.weatherManager.RainRate)
                {
                    explanation = "AbilityLowRain".Translate();
                    return false;
                }
                if (Props.maximumRainRate < map.weatherManager.RainRate)
                {
                    explanation = "AbilityHighRain".Translate();
                    return false;
                }
            }
            explanation = null;
            return true;
        }

        public bool CheckSnow(out string explanation)
        {
            Map map = parent.pawn.Map;
            if (map != null)
            {
                if (Props.minimumSnowRate > map.weatherManager.SnowRate)
                {
                    explanation = "AbilityLowSnow".Translate();
                    return false;
                }
                if (Props.maximumSnowRate < map.weatherManager.SnowRate)
                {
                    explanation = "AbilityHighSnow".Translate();
                    return false;
                }
            }
            explanation = null;
            return true;
        }

        // Caster specific
        public bool CheckCasterLight(out string explanation)
        {
            if (parent.pawn.Map == null)
            {
                if (Props.minCasterLightLevel > 0 || Props.maxCasterLightLevel < 1)
                {
                    explanation = "AbilityCasterLightLevel".Translate(Props.minCasterLightLevel.ToStringPercent(), Props.maxCasterLightLevel.ToStringPercent());
                    return false;
                }
            }
            else
            {
                float light = parent.pawn.Map.glowGrid.GroundGlowAt(parent.pawn.Position);
                if (light < Props.minCasterLightLevel || light > Props.maxCasterLightLevel)
                {
                    explanation = "AbilityCasterLightLevel".Translate(Props.minCasterLightLevel.ToStringPercent(), Props.maxCasterLightLevel.ToStringPercent());
                    return false;
                }
            }
            explanation = null;
            return true;
        }

        public bool CheckCasterHediffs(out string explanation)
        {
            if (parent.pawn.health == null || parent.pawn.health.hediffSet.hediffs.NullOrEmpty())
            {
                if (!Props.casterHasAllOfHediffs.NullOrEmpty())
                {
                    if (Props.casterHasAllOfHediffs.Count == 1 && Props.casterHasAnyOfHediffs.NullOrEmpty()) explanation = "AbilityNoCasterHediffOne".Translate(Props.casterHasAllOfHediffs[0].label);
                    else explanation = "AbilityNoCasterHediff".Translate();
                    return false;
                }
                if (!Props.casterHasAnyOfHediffs.NullOrEmpty())
                {
                    if (Props.casterHasAnyOfHediffs.Count == 1) explanation = "AbilityNoCasterHediffOne".Translate(Props.casterHasAnyOfHediffs[0].label);
                    else explanation = "AbilityNoCasterHediff".Translate();
                    return false;
                }
            }
            else
            {
                if (!Props.casterHasAllOfHediffs.NullOrEmpty() && !EBSGUtilities.PawnHasAllOfHediffs(parent.pawn, Props.casterHasAllOfHediffs))
                {
                    if (Props.casterHasAllOfHediffs.Count == 1 && Props.casterHasAnyOfHediffs.NullOrEmpty()) explanation = "AbilityNoCasterHediffOne".Translate(Props.casterHasAllOfHediffs[0].label);
                    else explanation = "AbilityNoCasterHediff".Translate();
                    return false;
                }
                if (!Props.casterHasAnyOfHediffs.NullOrEmpty() && !EBSGUtilities.PawnHasAnyOfHediffs(parent.pawn, Props.casterHasAnyOfHediffs))
                {
                    if (Props.casterHasAnyOfHediffs.Count == 1) explanation = "AbilityNoCasterHediffOne".Translate(Props.casterHasAnyOfHediffs[0].label);
                    else explanation = "AbilityNoCasterHediff".Translate();
                    return false;
                }
                if (!Props.casterHasNoneOfHediffs.NullOrEmpty() && EBSGUtilities.PawnHasAnyOfHediffs(parent.pawn, Props.casterHasNoneOfHediffs))
                {
                    if (Props.casterHasNoneOfHediffs.Count == 1) explanation = "AbilityCasterHediffOne".Translate(Props.casterHasNoneOfHediffs[0].label);
                    else explanation = "AbilityCasterHediff".Translate();
                    return false;
                }
            }

            explanation = null;
            return true;
        }

        public bool CheckCasterPawn(out string explanation)
        {
            Pawn pawn = parent.pawn;
            if (!Props.casterCapLimiters.NullOrEmpty())
            {
                foreach (CapCheck capCheck in Props.casterCapLimiters)
                {
                    if (pawn.health.capacities.CapableOf(capCheck.capacity))
                    {
                        if (capCheck.minCapValue > 0)
                        {
                            explanation = "AbilityCasterNoneCheck".Translate(capCheck.capacity.LabelCap);
                            return false;
                        }
                        continue;
                    }
                    float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                    if (capValue < capCheck.minCapValue)
                    {
                        explanation = "AbilityCasterLowCheck".Translate(capCheck.capacity.LabelCap);
                        return false;
                    }
                    if (capValue > capCheck.maxCapValue)
                    {
                        explanation = "AbilityCasterHighCheck".Translate(capCheck.capacity.LabelCap);
                        return false;
                    }
                }
            }
            if (!Props.casterStatLimiters.NullOrEmpty())
            {
                foreach (StatCheck statCheck in Props.casterStatLimiters)
                {
                    float statValue = pawn.GetStatValue(statCheck.stat);
                    if (statValue < statCheck.minStatValue)
                    {
                        explanation = "AbilityCasterLowCheck".Translate(statCheck.stat.LabelCap);
                        return false;
                    }
                    if (statValue > statCheck.maxStatValue)
                    {
                        explanation = "AbilityCasterHighCheck".Translate(statCheck.stat.LabelCap);
                        return false;
                    }
                }
            }
            if (!Props.casterSkillLimiters.NullOrEmpty())
            {
                foreach (SkillCheck skillCheck in Props.casterSkillLimiters)
                {
                    SkillRecord skill = pawn.skills.GetSkill(skillCheck.skill);
                    if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                    {
                        if (skillCheck.minLevel > 0)
                        {
                            explanation = "AbilityCasterNoneCheck".Translate(skillCheck.skill.LabelCap);
                            return false;
                        }
                        continue;
                    }
                    if (skill.Level < skillCheck.minLevel)
                    {
                        explanation = "AbilityCasterLowCheck".Translate(skillCheck.skill.LabelCap);
                        return false;
                    }
                    if (skill.Level > skillCheck.maxLevel)
                    {
                        explanation = "AbilityCasterHighCheck".Translate(skillCheck.skill.LabelCap);
                        return false;
                    }
                }
            }
            if (!Props.casterNeedLevels.NullOrEmpty())
            {
                if (!EBSGUtilities.AllNeedLevelsMet(pawn, Props.casterNeedLevels))
                {
                    explanation = "AbilityCasterNeedsCheck".Translate();
                    return false;
                }
            }

            explanation = null;
            return true;
        }

        // Caster map specific
        public bool CheckCondition(out string explanation)
        {
            if (parent.pawn.Map == null || parent.pawn.Map.GameConditionManager.ActiveConditions.NullOrEmpty())
            {
                if (!Props.requireOneOfCondition.NullOrEmpty())
                {
                    if (Props.requireOneOfCondition.Count == 1) explanation = "AbilityGameNoConditionOne".Translate(Props.requireOneOfCondition[0].label);
                    else explanation = "AbilityGameNoCondition".Translate();
                    return false;
                }
            }
            else
            {
                if (!Props.requireOneOfCondition.NullOrEmpty())
                {
                    bool flag = true;
                    foreach (GameConditionDef conditionDef in Props.requireOneOfCondition)
                    {
                        if (EBSGUtilities.ConditionOrExclusiveIsActive(conditionDef, parent.pawn.Map))
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        if (Props.requireOneOfCondition.Count == 1) explanation = "AbilityGameNoConditionOne".Translate(Props.requireOneOfCondition[0].label);
                        else explanation = "AbilityGameNoCondition".Translate();
                        return false;
                    }
                }
                if (!Props.forbiddenMapConditions.NullOrEmpty())
                {
                    foreach (GameConditionDef conditionDef in Props.forbiddenMapConditions)
                    {
                        if (parent.pawn.Map.GameConditionManager.ConditionIsActive(conditionDef))
                        {
                            explanation = "AbilityGameConditionOne".Translate(conditionDef.label);
                            return false;
                        }
                    }

                }
            }

            explanation = null;
            return true;
        }

        public bool CheckWeather(out string explanation)
        {
            if (parent.pawn.Map == null)
            {
                if (!Props.requireOneOfWeather.NullOrEmpty())
                {
                    if (Props.requireOneOfWeather.Count == 1) explanation = "AbilityNoWeatherOne".Translate(Props.requireOneOfWeather[0].label);
                    else explanation = "AbilityNoWeather".Translate();
                    return false;
                }
            }
            else
            {
                WeatherDef currentWeather = parent.pawn.Map.weatherManager.curWeather;
                if (!Props.requireOneOfWeather.NullOrEmpty() && !Props.requireOneOfWeather.Contains(currentWeather))
                {
                    if (Props.requireOneOfWeather.Count == 1) explanation = "AbilityNoWeatherOne".Translate(Props.requireOneOfWeather[0].label);
                    else explanation = "AbilityNoWeather".Translate();
                    return false;
                }
                if (!Props.forbiddenWeather.NullOrEmpty() && Props.forbiddenWeather.Contains(currentWeather))
                {
                    explanation = "AbilityWeatherOne".Translate(currentWeather.label);
                    return false;
                }
            }

            explanation = null;
            return true;
        }

        public bool CheckHour(out string explanation)
        {
            if (Props.minPartOfDay <= 0 && Props.maxPartOfDay >= 1)
            {
                explanation = null;
                return true;
            }

            float time = GenLocalDate.DayPercent(parent.pawn);
            if (time < Props.minPartOfDay || time > Props.maxPartOfDay)
            {
                int minHour = GenDate.HourOfDay((long)(Props.minPartOfDay * 60000), Find.WorldGrid.LongLatOf(parent.pawn.Tile).x);
                int maxHour = GenDate.HourOfDay((long)((Props.maxPartOfDay + 0.1f) * 60000), Find.WorldGrid.LongLatOf(parent.pawn.Tile).x);
                explanation = "AbilityTime".Translate(minHour.ToString(), maxHour.ToString(), parent.pawn);
                return false;
            }

            explanation = null;
            return true;
        }


        // Target specific
        public bool CheckTargetLight(LocalTargetInfo target, out string explanation)
        {
            Map map;
            if (target.HasThing) map = target.Thing.Map;
            else map = parent.pawn.Map;

            if (map == null)
            {
                if (Props.minTargetLightLevel > 0 || Props.maxTargetLightLevel < 1)
                {
                    explanation = "AbilityTargetLightLevel".Translate(Props.minTargetLightLevel.ToStringPercent(), Props.maxTargetLightLevel.ToStringPercent());
                    return false;
                }
            }
            else
            {
                float light = map.glowGrid.GroundGlowAt(target.Cell);
                if (light < Props.minTargetLightLevel || light > Props.maxTargetLightLevel)
                {
                    explanation = "AbilityTargetLightLevel".Translate(Props.minTargetLightLevel.ToStringPercent(), Props.maxTargetLightLevel.ToStringPercent());
                    return false;
                }
            }
            explanation = null;
            return true;
        }

        public bool CheckTargetHediffs(LocalTargetInfo target, out string explanation)
        {
            if (EBSGUtilities.TargetIsPawn(target, out Pawn targetPawn))
            {
                if (targetPawn.health == null || targetPawn.health.hediffSet.hediffs.NullOrEmpty())
                {
                    if (!Props.targetHasAllOfHediffs.NullOrEmpty())
                    {
                        if (Props.targetHasAllOfHediffs.Count == 1 && Props.targetHasAllOfHediffs.NullOrEmpty()) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAllOfHediffs[0].label);
                        else explanation = "AbilityNoTargetHediff".Translate();
                        return false;
                    }
                    if (!Props.targetHasAnyOfHediffs.NullOrEmpty())
                    {
                        if (Props.targetHasAnyOfHediffs.Count == 1) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAnyOfHediffs[0].label);
                        else explanation = "AbilityNoTargetHediff".Translate();
                        return false;
                    }
                }
                else
                {
                    if (!Props.targetHasAllOfHediffs.NullOrEmpty() && !EBSGUtilities.PawnHasAllOfHediffs(targetPawn, Props.targetHasAllOfHediffs))
                    {
                        if (Props.targetHasAllOfHediffs.Count == 1 && Props.targetHasAnyOfHediffs.NullOrEmpty()) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAllOfHediffs[0].label);
                        else explanation = "AbilityNoTargetHediff".Translate();
                        return false;
                    }
                    if (!Props.targetHasAnyOfHediffs.NullOrEmpty() && !EBSGUtilities.PawnHasAnyOfHediffs(targetPawn, Props.targetHasAnyOfHediffs))
                    {
                        if (Props.targetHasAnyOfHediffs.Count == 1) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAnyOfHediffs[0].label);
                        else explanation = "AbilityNoTargetHediff".Translate();
                        return false;
                    }
                    if (!Props.targetHasNoneOfHediffs.NullOrEmpty() && EBSGUtilities.PawnHasAnyOfHediffs(targetPawn, Props.targetHasNoneOfHediffs))
                    {
                        if (Props.targetHasNoneOfHediffs.Count == 1) explanation = "AbilityTargetHediffOne".Translate(Props.targetHasNoneOfHediffs[0].label);
                        else explanation = "AbilityTargetHediff".Translate();
                        return false;
                    }
                }
            }
            else
            {
                if (!Props.targetHasAllOfHediffs.NullOrEmpty())
                {
                    if (Props.targetHasAllOfHediffs.Count == 1 && Props.targetHasAllOfHediffs.NullOrEmpty()) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAllOfHediffs[0].label);
                    else explanation = "AbilityNoTargetHediff".Translate();
                    return false;
                }
                if (!Props.targetHasAnyOfHediffs.NullOrEmpty())
                {
                    if (Props.targetHasAnyOfHediffs.Count == 1) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAnyOfHediffs[0].label);
                    else explanation = "AbilityNoTargetHediff".Translate();
                    return false;
                }
            }

            explanation = null;
            return true;
        }

        public bool CheckTargetGenes(LocalTargetInfo target, out string explanation)
        {
            if (EBSGUtilities.TargetIsPawn(target, out Pawn targetPawn))
            {
                if (targetPawn.genes == null || targetPawn.genes.GenesListForReading.NullOrEmpty())
                {
                    if (!Props.targetHasAllOfGenes.NullOrEmpty())
                    {
                        if (Props.targetHasAllOfGenes.Count == 1 && Props.targetHasAllOfGenes.NullOrEmpty()) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAllOfGenes[0].label);
                        else explanation = "AbilityNoTargetGene".Translate();
                        return false;
                    }
                    if (!Props.targetHasAnyOfGenes.NullOrEmpty())
                    {
                        if (Props.targetHasAnyOfGenes.Count == 1) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAnyOfGenes[0].label);
                        else explanation = "AbilityNoTargetGene".Translate();
                        return false;
                    }
                }
                else
                {
                    if (!Props.targetHasAllOfGenes.NullOrEmpty() && !EBSGUtilities.PawnHasAllOfGenes(targetPawn, Props.targetHasAllOfGenes))
                    {
                        if (Props.targetHasAllOfGenes.Count == 1 && Props.targetHasAnyOfGenes.NullOrEmpty()) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAllOfGenes[0].label);
                        else explanation = "AbilityNoTargetGene".Translate();
                        return false;
                    }
                    if (!Props.targetHasAnyOfGenes.NullOrEmpty() && !EBSGUtilities.PawnHasAnyOfGenes(targetPawn, out var anyOfGene, Props.targetHasAnyOfGenes))
                    {
                        if (Props.targetHasAnyOfGenes.Count == 1) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAnyOfGenes[0].label);
                        else explanation = "AbilityNoTargetGene".Translate();
                        return false;
                    }
                    if (!Props.targetHasNoneOfGenes.NullOrEmpty() && EBSGUtilities.PawnHasAnyOfGenes(targetPawn, out var noneOfGene, Props.targetHasNoneOfGenes))
                    {
                        if (Props.targetHasNoneOfGenes.Count == 1) explanation = "AbilityTargetGeneOne".Translate(Props.targetHasNoneOfGenes[0].label);
                        else explanation = "AbilityTargetGene".Translate();
                        return false;
                    }
                }
            }
            else
            {
                if (!Props.targetHasAllOfGenes.NullOrEmpty())
                {
                    if (Props.targetHasAllOfGenes.Count == 1 && Props.targetHasAllOfGenes.NullOrEmpty()) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAllOfGenes[0].label);
                    else explanation = "AbilityNoTargetGene".Translate();
                    return false;
                }
                if (!Props.targetHasAnyOfGenes.NullOrEmpty())
                {
                    if (Props.targetHasAnyOfGenes.Count == 1) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAnyOfGenes[0].label);
                    else explanation = "AbilityNoTargetGene".Translate();
                    return false;
                }
            }

            explanation = null;
            return true;
        }

        public bool CheckTargetPawn(LocalTargetInfo target, out string explanation)
        {
            if (EBSGUtilities.TargetIsPawn(target, out Pawn pawn))
            {
                if (!Props.targetCapLimiters.NullOrEmpty())
                {
                    foreach (CapCheck capCheck in Props.targetCapLimiters)
                    {
                        if (!pawn.health.capacities.CapableOf(capCheck.capacity))
                        {
                            if (capCheck.minCapValue > 0)
                            {
                                explanation = "AbilityTargetNoneCheck".Translate(capCheck.capacity.LabelCap);
                                return false;
                            }
                            continue;
                        }
                        float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                        if (capValue < capCheck.minCapValue)
                        {
                            explanation = "AbilityTargetLowCheck".Translate(capCheck.capacity.LabelCap);
                            return false;
                        }
                        if (capValue > capCheck.maxCapValue)
                        {
                            explanation = "AbilityTargetHighCheck".Translate(capCheck.capacity.LabelCap);
                            return false;
                        }
                    }
                }
                if (!Props.targetStatLimiters.NullOrEmpty())
                {
                    foreach (StatCheck statCheck in Props.targetStatLimiters)
                    {
                        float statValue = pawn.GetStatValue(statCheck.stat);
                        if (statValue < statCheck.minStatValue)
                        {
                            explanation = "AbilityTargetLowCheck".Translate(statCheck.stat.LabelCap);
                            return false;
                        }
                        if (statValue > statCheck.maxStatValue)
                        {
                            explanation = "AbilityTargetHighCheck".Translate(statCheck.stat.LabelCap);
                            return false;
                        }
                    }
                }
                if (!Props.targetSkillLimiters.NullOrEmpty())
                {
                    foreach (SkillCheck skillCheck in Props.targetSkillLimiters)
                    {
                        SkillRecord skill = pawn.skills.GetSkill(skillCheck.skill);
                        if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                        {
                            if (skillCheck.minLevel > 0)
                            {
                                explanation = "AbilityTargetNoneCheck".Translate(skillCheck.skill.LabelCap);
                                return false;
                            }
                            continue;
                        }
                        if (skill.Level < skillCheck.minLevel)
                        {
                            explanation = "AbilityTargetLowCheck".Translate(skillCheck.skill.LabelCap);
                            return false;
                        }
                        if (skill.Level > skillCheck.maxLevel)
                        {
                            explanation = "AbilityTargetHighCheck".Translate(skillCheck.skill.LabelCap);
                            return false;
                        }
                    }
                }
                if (!Props.targetNeedLevels.NullOrEmpty())
                {
                    if (!EBSGUtilities.AllNeedLevelsMet(pawn, Props.targetNeedLevels))
                    {
                        explanation = "AbilityTargetNeedsCheck".Translate();
                        return false;
                    }
                }
            }
            else
            {
                if (!Props.targetCapLimiters.NullOrEmpty())
                {
                    foreach (CapCheck capCheck in Props.targetCapLimiters)
                    {
                        if (capCheck.minCapValue > 0)
                        {
                            explanation = "AbilityTargetMustBePawn".Translate();
                            return false;
                        }
                    }
                }
                if (!Props.targetSkillLimiters.NullOrEmpty())
                {
                    foreach (SkillCheck skillCheck in Props.targetSkillLimiters)
                    {
                        if (skillCheck.minLevel > 0)
                        {
                            explanation = "AbilityTargetMustBePawn".Translate();
                            return false;
                        }
                    }
                }
                if (!Props.targetStatLimiters.NullOrEmpty())
                {
                    Thing thing = null;
                    if (target.HasThing) thing = target.Thing;
                    foreach (StatCheck statCheck in Props.targetStatLimiters)
                    {
                        if (thing == null)
                        {
                            if (statCheck.minStatValue > 0)
                            {
                                explanation = "AbilityTargetMustBePawn".Translate();
                                return false;
                            }
                        }
                        else
                        {
                            float statValue = thing.GetStatValue(statCheck.stat);
                            if (statValue < statCheck.minStatValue)
                            {
                                explanation = "AbilityTargetLowCheck".Translate(statCheck.stat.LabelCap);
                                return false;
                            }
                            if (statValue > statCheck.maxStatValue)
                            {
                                explanation = "AbilityTargetHighCheck".Translate(statCheck.stat.LabelCap);
                                return false;
                            }
                        }
                    }
                }
                if (!Props.targetNeedLevels.NullOrEmpty())
                {
                    foreach (NeedLevel needLevel in Props.targetNeedLevels)
                        if (needLevel.minNeedLevel > 0)
                        {
                            explanation = "AbilityTargetMustBePawn".Translate();
                            return false;
                        }
                }
            }

            explanation = null;
            return true;
        }
    }
}

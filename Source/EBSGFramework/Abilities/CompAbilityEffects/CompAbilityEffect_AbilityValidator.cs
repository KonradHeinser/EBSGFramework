﻿using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_AbilityValidator : CompAbilityEffect
    {
        public new CompProperties_AbilityAbilityValidator Props => (CompProperties_AbilityAbilityValidator)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, !Props.noMessage);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages)) return false;
            string baseExplanation = "CannotUseAbility".Translate(parent.def.label) + ": ";

            // Caster checks
            if (!CheckCasterLight(out string casterLightExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + casterLightExplanation, parent.pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckCasterRoof(out string casterRoofExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + casterRoofExplanation, parent.pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckCasterHediffs(out string casterHediffExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + casterHediffExplanation, parent.pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckCasterPawn(out string casterExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + casterExplanation, parent.pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Caster map checks
            if (!CheckRain(out string rainExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + rainExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!CheckSnow(out string snowExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + snowExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                return false;
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
            if (!CheckTargetRoof(target, out string targetRoofExplanation))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + targetRoofExplanation, target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
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
                return false;
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
                if (!CheckCasterRoof(out string casterRoofExplanation))
                {
                    reason = casterRoofExplanation;
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

        public override bool ShouldHideGizmo => GizmoDisabled(out _) && Props.hideGizmo;

        public bool CheckRain(out string explanation)
        {
            Map map = parent.pawn.Map;
            if (map != null)
            {
                var rainRate = map.weatherManager.RainRate;
                if (rainRate < Props.rainRate.min)
                {
                    explanation = "AbilityLowRain".Translate();
                    return false;
                }
                if (rainRate > Props.rainRate.max && (!Props.checkRoofForRainSnowRate || !parent.pawn.Position.Roofed(map)))
                {
                    explanation = "AbilityHighRain".Translate();
                    return false;
                }
                if (Props.checkRoofForRainSnowRate && Props.rainRate.min >= 0 && parent.pawn.Position.Roofed(map) )
                {
                    explanation = "Roofed".Translate();
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
                var snowRate = map.weatherManager.SnowRate;

                if (Props.snowRate.min > snowRate)
                {
                    explanation = "AbilityLowSnow".Translate();
                    return false;
                }
                if (Props.snowRate.max < snowRate && (!Props.checkRoofForRainSnowRate || !parent.pawn.Position.Roofed(map)))
                {
                    explanation = "AbilityHighSnow".Translate();
                    return false;
                }
                if (Props.checkRoofForRainSnowRate && Props.snowRate.min >= 0 && parent.pawn.Position.Roofed(map))
                {
                    explanation = "Roofed".Translate();
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
                if (Props.casterLightLevel != FloatRange.ZeroToOne)
                {
                    explanation = "AbilityCasterLightLevel".Translate(Props.casterLightLevel.min.ToStringPercent(), Props.casterLightLevel.max.ToStringPercent());
                    return false;
                }
            }
            else
            {
                float light = parent.pawn.Map.glowGrid.GroundGlowAt(parent.pawn.Position);
                if (Props.casterLightLevel.Includes(light) == Props.invertCasterLight)
                {
                    var translate = Props.invertCasterLight ? "AbilityCasterLightLevelInvert" : "AbilityCasterLightLevel";
                    explanation = translate.Translate(Props.casterLightLevel.min.ToStringPercent(), Props.casterLightLevel.max.ToStringPercent());
                    return false;
                }
            }
            explanation = null;
            return true;
        }

        public bool CheckCasterRoof(out string explanation)
        {
            explanation = null;

            if (Props.casterRoof != RoofCheck.NoCheck && parent.pawn.Spawned)
            {
                RoofDef roof = parent.pawn.PositionHeld.GetRoof(parent.pawn.MapHeld);
                switch (Props.casterRoof)
                {
                    case RoofCheck.AnyRoof:
                        if (roof == null)
                        {
                            explanation = "AbilityCasterRoof".Translate();
                            return false;
                        }
                        break;
                    case RoofCheck.ThickRoof:
                        if (roof?.isThickRoof != true)
                        {
                            explanation = "AbilityCasterThickRoof".Translate();
                            return false;
                        }
                        break;
                    case RoofCheck.NoRoof:
                        if (roof != null)
                        {
                            explanation = "AbilityCasterNoRoof".Translate();
                            return false;
                        }
                        break;
                    case RoofCheck.NoThickRoof:
                        if (roof?.isThickRoof == true)
                        {
                            explanation = "AbilityCasterNoThickRoof".Translate();
                            return false;
                        }
                        break;
                }
            }

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
                if (!Props.casterHasAllOfHediffs.NullOrEmpty() && !parent.pawn.PawnHasAllOfHediffs(Props.casterHasAllOfHediffs))
                {
                    if (Props.casterHasAllOfHediffs.Count == 1 && Props.casterHasAnyOfHediffs.NullOrEmpty()) explanation = "AbilityNoCasterHediffOne".Translate(Props.casterHasAllOfHediffs[0].label);
                    else explanation = "AbilityNoCasterHediff".Translate();
                    return false;
                }
                if (!Props.casterHasAnyOfHediffs.NullOrEmpty() && !parent.pawn.PawnHasAnyOfHediffs(Props.casterHasAnyOfHediffs))
                {
                    if (Props.casterHasAnyOfHediffs.Count == 1) explanation = "AbilityNoCasterHediffOne".Translate(Props.casterHasAnyOfHediffs[0].label);
                    else explanation = "AbilityNoCasterHediff".Translate();
                    return false;
                }
                if (!Props.casterHasNoneOfHediffs.NullOrEmpty() && parent.pawn.PawnHasAnyOfHediffs(Props.casterHasNoneOfHediffs))
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
                    if (!pawn.health.capacities.CapableOf(capCheck.capacity))
                    {
                        if (capCheck.range.min > 0)
                        {
                            explanation = "AbilityCasterNoneCheck".Translate(capCheck.capacity.LabelCap);
                            return false;
                        }
                        continue;
                    }
                    float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                    if (capValue < capCheck.range.min)
                    {
                        explanation = "AbilityCasterLowCheck".Translate(capCheck.capacity.LabelCap);
                        return false;
                    }
                    if (capValue > capCheck.range.max)
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
                    float statValue = pawn.StatOrOne(statCheck.stat);
                    if (statValue < statCheck.range.min)
                    {
                        explanation = "AbilityCasterLowCheck".Translate(statCheck.stat.LabelCap);
                        return false;
                    }
                    if (statValue > statCheck.range.max)
                    {
                        explanation = "AbilityCasterHighCheck".Translate(statCheck.stat.LabelCap);
                        return false;
                    }
                }
            }
            if (!Props.casterSkillLimiters.NullOrEmpty())
            {
                foreach (SkillLevel skillLevel in Props.casterSkillLimiters)
                {
                    SkillRecord skill = pawn.skills?.GetSkill(skillLevel.skill);
                    if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                    {
                        if (skillLevel.range.min > 0)
                        {
                            explanation = "AbilityCasterNoneCheck".Translate(skill.def.LabelCap);
                            return false;
                        }
                        continue;
                    }
                    if (skill.Level < skillLevel.range.min)
                    {
                        explanation = "AbilityCasterLowCheck".Translate(skill.def.LabelCap);
                        return false;
                    }
                    if (skill.Level > skillLevel.range.max)
                    {
                        explanation = "AbilityCasterHighCheck".Translate(skill.def.LabelCap);
                        return false;
                    }
                }
            }
            if (!Props.casterNeedLevels.NullOrEmpty())
            {
                if (!pawn.AllNeedLevelsMet(Props.casterNeedLevels))
                {
                    explanation = "AbilityCasterNeedsCheck".Translate();
                    return false;
                }
            }
            if (!Props.validCasterFactions.NullOrEmpty())
            {
                if (pawn.Faction == null || !Props.validCasterFactions.Contains(pawn.Faction.def))
                {
                    if (Props.validCasterFactions.Count == 1)
                        explanation = "AbilityCasterFactionOne".Translate(Props.validCasterFactions[0].LabelCap);
                    else
                        explanation = "AbilityCasterFaction".Translate();
                    return false;
                }
            }
            if (!Props.forbiddenCasterFactions.NullOrEmpty())
            {
                if (pawn.Faction != null && Props.forbiddenCasterFactions.Contains(pawn.Faction.def))
                {
                    explanation = "AbilityCasterNoFaction".Translate(pawn.Faction.Name);
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
                        if (conditionDef.ConditionOrExclusiveIsActive(parent.pawn.Map))
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
            float time = GenLocalDate.DayPercent(parent.pawn);
            if (Props.progressThroughDay.Includes(time) == Props.invertTime)
            {
                int minHour = GenDate.HourOfDay((long)(Props.progressThroughDay.min * 60000), Find.WorldGrid.LongLatOf(parent.pawn.Tile).x);
                int maxHour = GenDate.HourOfDay((long)((Props.progressThroughDay.max + 0.1f) * 60000), Find.WorldGrid.LongLatOf(parent.pawn.Tile).x);
                if (!Props.invertTime)
                    explanation = "AbilityTime".Translate(minHour.ToString(), maxHour.ToString(), parent.pawn);
                else
                    explanation = "AbilityTime".Translate(maxHour.ToString(), minHour.ToString(), parent.pawn);
                return false;
            }

            if (!parent.pawn.CheckSeason(Props.seasons, false))
            {
                explanation = "EBSG_SeasonWrong".Translate();
                return false;
            }

            explanation = null;
            return true;
        }


        // Target specific
        public bool CheckTargetLight(LocalTargetInfo target, out string explanation)
        {
            explanation = null;
            Map map = target.Thing?.MapHeld ?? parent.pawn.MapHeld;
            if (!target.Cell.InBounds(map))
                return false;

            if (map == null)
            {
                if (Props.targetLightLevel != FloatRange.ZeroToOne)
                {
                    explanation = "AbilityTargetLightLevel".Translate(Props.targetLightLevel.min.ToStringPercent(), Props.targetLightLevel.max.ToStringPercent());
                    return false;
                }
            }
            else
            {
                float light = map.glowGrid.GroundGlowAt(target.Cell);
                if (Props.targetLightLevel.Includes(light) == Props.invertTargetLight)
                {
                    var translate = Props.invertTargetLight ? "AbilityTargetLightLevelInvert" : "AbilityTargetLightLevel";
                    explanation = translate.Translate(Props.targetLightLevel.min.ToStringPercent(), Props.targetLightLevel.max.ToStringPercent());
                    return false;
                }
            }
            return true;
        }

        public bool CheckTargetRoof(LocalTargetInfo target, out string explanation)
        {
            explanation = null;
            if (parent.pawn.MapHeld == null) return true;
            IntVec3 pos = target.Cell;
            Map map = parent.pawn.MapHeld;

            if (Props.targetRoof != RoofCheck.NoCheck)
            {
                RoofDef roof = pos.GetRoof(map);
                switch (Props.targetRoof)
                {
                    case RoofCheck.AnyRoof:
                        if (roof == null)
                        {
                            explanation = "AbilityTargetRoof".Translate();
                            return false;
                        }
                        break;
                    case RoofCheck.ThickRoof:
                        if (roof?.isThickRoof != true)
                        {
                            explanation = "AbilityTargetThickRoof".Translate();
                            return false;
                        }
                        break;
                    case RoofCheck.NoRoof:
                        if (roof != null)
                        {
                            explanation = "AbilityTargetNoRoof".Translate();
                            return false;
                        }
                        break;
                    case RoofCheck.NoThickRoof:
                        if (roof?.isThickRoof == true)
                        {
                            explanation = "AbilityTargetNoThickRoof".Translate();
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        public bool CheckTargetHediffs(LocalTargetInfo target, out string explanation)
        {
            if (target.TargetIsPawn(out Pawn targetPawn))
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
                    if (!Props.targetHasAllOfHediffs.NullOrEmpty() && !targetPawn.PawnHasAllOfHediffs(Props.targetHasAllOfHediffs))
                    {
                        if (Props.targetHasAllOfHediffs.Count == 1 && Props.targetHasAnyOfHediffs.NullOrEmpty()) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAllOfHediffs[0].label);
                        else explanation = "AbilityNoTargetHediff".Translate();
                        return false;
                    }
                    if (!Props.targetHasAnyOfHediffs.NullOrEmpty() && !targetPawn.PawnHasAnyOfHediffs(Props.targetHasAnyOfHediffs))
                    {
                        if (Props.targetHasAnyOfHediffs.Count == 1) explanation = "AbilityNoTargetHediffOne".Translate(Props.targetHasAnyOfHediffs[0].label);
                        else explanation = "AbilityNoTargetHediff".Translate();
                        return false;
                    }
                    if (!Props.targetHasNoneOfHediffs.NullOrEmpty() && targetPawn.PawnHasAnyOfHediffs(Props.targetHasNoneOfHediffs))
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
            if (target.TargetIsPawn(out Pawn targetPawn))
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
                    if (targetPawn.genes.Xenotype != null)
                    {
                        if (!Props.targetIsNoneOfXenotype.NullOrEmpty() && Props.targetIsNoneOfXenotype.Contains(targetPawn.genes.Xenotype))
                        {
                            explanation = "AbilityTargetXenotype".Translate(targetPawn.genes.Xenotype.label);
                            return false;
                        }
                        if (!Props.targetIsOneOfXenotype.NullOrEmpty() && !Props.targetIsOneOfXenotype.Contains(targetPawn.genes.Xenotype))
                        {
                            if (Props.targetIsOneOfXenotype.Count == 1) explanation = "AbilityTargetXenotypeOne".Translate(Props.targetIsOneOfXenotype[0].label);
                            else explanation = "AbilityTargetXenotype".Translate(targetPawn.genes.Xenotype.label);
                            return false;
                        }
                    }
                    if (!Props.targetHasAllOfGenes.NullOrEmpty() && !targetPawn.PawnHasAllOfGenes(Props.targetHasAllOfGenes))
                    {
                        if (Props.targetHasAllOfGenes.Count == 1 && Props.targetHasAnyOfGenes.NullOrEmpty()) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAllOfGenes[0].label);
                        else explanation = "AbilityNoTargetGene".Translate();
                        return false;
                    }
                    if (!Props.targetHasAnyOfGenes.NullOrEmpty() && !targetPawn.PawnHasAnyOfGenes(out var anyOfGene, Props.targetHasAnyOfGenes))
                    {
                        if (Props.targetHasAnyOfGenes.Count == 1) explanation = "AbilityNoTargetGeneOne".Translate(Props.targetHasAnyOfGenes[0].label);
                        else explanation = "AbilityNoTargetGene".Translate();
                        return false;
                    }
                    if (!Props.targetHasNoneOfGenes.NullOrEmpty() && targetPawn.PawnHasAnyOfGenes(out var noneOfGene, Props.targetHasNoneOfGenes))
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
            if (target.TargetIsPawn(out Pawn pawn))
            {
                if (!Props.targetCapLimiters.NullOrEmpty())
                {
                    foreach (CapCheck capCheck in Props.targetCapLimiters)
                    {
                        if (!pawn.health.capacities.CapableOf(capCheck.capacity))
                        {
                            if (capCheck.range.min > 0)
                            {
                                explanation = "AbilityTargetNoneCheck".Translate(capCheck.capacity.LabelCap);
                                return false;
                            }
                            continue;
                        }
                        float capValue = pawn.health.capacities.GetLevel(capCheck.capacity);
                        if (capValue < capCheck.range.min)
                        {
                            explanation = "AbilityTargetLowCheck".Translate(capCheck.capacity.LabelCap);
                            return false;
                        }
                        if (capValue > capCheck.range.max)
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
                        float statValue = pawn.StatOrOne(statCheck.stat);
                        if (statValue < statCheck.range.min)
                        {
                            explanation = "AbilityTargetLowCheck".Translate(statCheck.stat.LabelCap);
                            return false;
                        }
                        if (statValue > statCheck.range.max)
                        {
                            explanation = "AbilityTargetHighCheck".Translate(statCheck.stat.LabelCap);
                            return false;
                        }
                    }
                }
                if (!Props.targetSkillLimiters.NullOrEmpty())
                {
                    foreach (SkillLevel skillLevel in Props.targetSkillLimiters)
                    {
                        SkillRecord skill = pawn.skills?.GetSkill(skillLevel.skill);
                        if (skill == null || skill.TotallyDisabled || skill.PermanentlyDisabled)
                        {
                            if (skillLevel.range.min > 0)
                            {
                                explanation = "AbilityTargetNoneCheck".Translate(skill.def.LabelCap);
                                return false;
                            }
                            continue;
                        }
                        if (skill.Level < skillLevel.range.min)
                        {
                            explanation = "AbilityTargetLowCheck".Translate(skill.def.LabelCap);
                            return false;
                        }
                        if (skill.Level > skillLevel.range.max)
                        {
                            explanation = "AbilityTargetHighCheck".Translate(skill.def.LabelCap);
                            return false;
                        }
                    }
                }
                if (!Props.targetNeedLevels.NullOrEmpty())
                {
                    if (!pawn.AllNeedLevelsMet(Props.targetNeedLevels))
                    {
                        explanation = "AbilityTargetNeedsCheck".Translate();
                        return false;
                    }
                }
                if (Props.invertBodySize)
                {
                    if (!Props.bodySize.Includes(pawn.BodySize))
                    {
                        explanation = "TargetSizeInvert".Translate(Props.bodySize.min, Props.bodySize.max);
                        return false;
                    }
                }
                else
                {
                    if (pawn.BodySize < Props.bodySize.min)
                    {
                        explanation = "TargetTooSmall".Translate();
                        return false;
                    }
                    if (pawn.BodySize > Props.bodySize.max)
                    {
                        explanation = "TargetTooLarge".Translate();
                        return false;
                    }
                }
                
                if (!Props.targetPawnKinds.NullOrEmpty() && !Props.targetPawnKinds.Contains(pawn.kindDef))
                {
                    explanation = "AbilityPawnKind".Translate();
                    return false;
                }
                if (Props.targetGroup != TargetGroup.None)
                {
                    switch (Props.targetGroup)
                    {
                        case TargetGroup.Player:
                            if (pawn.Faction?.IsPlayer != true)
                            {
                                explanation = "AbilityTargetPlayer".Translate();
                                return false;
                            }
                            break;
                        case TargetGroup.NonPlayer:
                            if (pawn.Faction?.IsPlayer == true)
                            {
                                explanation = "AbilityTargetNonPlayer".Translate();
                                return false;
                            }
                            break;
                        case TargetGroup.Hostiles:
                            if (pawn.Faction?.HostileTo(Faction.OfPlayer) != true && !pawn.HostileTo(parent.pawn))
                            {
                                explanation = "AbilityTargetHostile".Translate();
                                return false;
                            }
                            break;
                        case TargetGroup.NoFaction:
                            if (pawn.Faction != null)
                            {
                                explanation = "AbilityTargetFactionNone".Translate();
                                return false;
                            }
                            break;
                    }
                }
                if (!Props.validTargetFactions.NullOrEmpty())
                {
                    if (pawn.Faction == null || !Props.validTargetFactions.Contains(pawn.Faction.def))
                    {
                        if (Props.validTargetFactions.Count == 1)
                            explanation = "AbilityTargetFactionOne".Translate(Props.validTargetFactions[0].LabelCap);
                        else
                            explanation = "AbilityTargetFaction".Translate();
                        return false;
                    }
                }
                if (!Props.forbiddenTargetFactions.NullOrEmpty())
                {
                    if (pawn.Faction != null && Props.forbiddenTargetFactions.Contains(pawn.Faction.def))
                    {
                        explanation = "AbilityTargetNoFaction".Translate(pawn.Faction.Name);
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
                        if (capCheck.range.min > 0)
                        {
                            explanation = "AbilityTargetMustBePawn".Translate();
                            return false;
                        }
                    }
                }
                if (!Props.targetSkillLimiters.NullOrEmpty())
                {
                    foreach (SkillLevel skillLevel in Props.targetSkillLimiters)
                    {
                        if (skillLevel.range.min > 0)
                        {
                            explanation = "AbilityTargetMustBePawn".Translate();
                            return false;
                        }
                    }
                }
                if (!Props.targetStatLimiters.NullOrEmpty())
                {
                    Thing thing = target.Thing;
                    foreach (StatCheck statCheck in Props.targetStatLimiters)
                    {
                        if (thing == null)
                        {
                            if (statCheck.range.min > 0)
                            {
                                explanation = "AbilityTargetMustBePawn".Translate();
                                return false;
                            }
                        }
                        else
                        {
                            float statValue = thing.StatOrOne(statCheck.stat);
                            if (statValue < statCheck.range.min)
                            {
                                explanation = "AbilityTargetLowCheck".Translate(statCheck.stat.LabelCap);
                                return false;
                            }
                            if (statValue > statCheck.range.max)
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
                        if (needLevel.range.min > 0)
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

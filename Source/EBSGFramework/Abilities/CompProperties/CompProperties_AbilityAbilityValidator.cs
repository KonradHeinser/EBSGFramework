using System.Collections.Generic;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityAbilityValidator : CompProperties_AbilityEffect
    {
        // Adds custom requirements for an ability to be usable on a target
        public bool disableGizmo = true; // Disables gizmo when caster or map conditions prevent the ability from working

        // Target Genes
        public List<GeneDef> targetHasAnyOfGenes;
        public List<GeneDef> targetHasAllOfGenes;
        public List<GeneDef> targetHasNoneOfGenes;
        public List<XenotypeDef> targetIsOneOfXenotype;
        public List<XenotypeDef> targetIsNoneOfXenotype;

        // Target Hediffs
        public List<HediffDef> targetHasAnyOfHediffs;
        public List<HediffDef> targetHasAllOfHediffs;
        public List<HediffDef> targetHasNoneOfHediffs;

        // Target Pawn Checks
        public List<CapCheck> targetCapLimiters;
        public List<SkillCheck> targetSkillLimiters;
        public List<StatCheck> targetStatLimiters;
        public List<NeedLevel> targetNeedLevels;
        public float minBodySize = 0f;
        public float maxBodySize = 999f;

        // Caster Hediffs
        public List<HediffDef> casterHasAnyOfHediffs;
        public List<HediffDef> casterHasAllOfHediffs;
        public List<HediffDef> casterHasNoneOfHediffs;

        // Caster Pawn Checks
        public List<CapCheck> casterCapLimiters;
        public List<SkillCheck> casterSkillLimiters;
        public List<StatCheck> casterStatLimiters;
        public List<NeedLevel> casterNeedLevels;

        // % Light
        public float minTargetLightLevel = 0f;
        public float maxTargetLightLevel = 1f;
        public float minCasterLightLevel = 0f;
        public float maxCasterLightLevel = 1f;

        // % of progress through the day
        public float minPartOfDay = 0f;
        public float maxPartOfDay = 1f;

        // Map Condition
        public float minimumRainRate = 0f;
        public float maximumRainRate = 9999f;
        public float minimumSnowRate = 0f;
        public float maximumSnowRate = 9999f;

        public List<WeatherDef> requireOneOfWeather;
        public List<WeatherDef> forbiddenWeather;
        public List<GameConditionDef> requireOneOfCondition;
        public List<GameConditionDef> forbiddenMapConditions;

        public CompProperties_AbilityAbilityValidator()
        {
            compClass = typeof(CompAbilityEffect_AbilityValidator);
        }
    }
}

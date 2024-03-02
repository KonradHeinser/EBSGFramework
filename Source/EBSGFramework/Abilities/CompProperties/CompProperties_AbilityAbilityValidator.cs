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

        // Target Hediffs
        public List<HediffDef> targetHasAnyOfHediffs;
        public List<HediffDef> targetHasAllOfHediffs;
        public List<HediffDef> targetHasNoneOfHediffs;

        // Target Pawn Checks
        public List<CapCheck> targetCapLimiters;
        public List<SkillCheck> targetSkillLimiters;
        public List<StatCheck> targetStatLimiters;

        // Caster Hediffs
        public List<HediffDef> casterHasAnyOfHediffs;
        public List<HediffDef> casterHasAllOfHediffs;
        public List<HediffDef> casterHasNoneOfHediffs;

        // Target Pawn Checks
        public List<CapCheck> casterCapLimiters;
        public List<SkillCheck> casterSkillLimiters;
        public List<StatCheck> casterStatLimiters;

        // % Light
        public float minTargetLightLevel = 0f;
        public float maxTargetLightLevel = 1f;
        public float minCasterLightLevel = 0f;
        public float maxCasterLightLevel = 1f;

        // % of progress through the day
        public float minPartOfDay = 0f;
        public float maxPartOfDay = 1f;

        // Map Condition
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

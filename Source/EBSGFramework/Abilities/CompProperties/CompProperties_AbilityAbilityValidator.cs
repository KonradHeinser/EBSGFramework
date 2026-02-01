using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityAbilityValidator : CompProperties_AbilityEffect
    {
        // Adds custom requirements for an ability to be usable on a target
        public bool disableGizmo = true; // Disables gizmo when caster or map conditions prevent the ability from working
        public bool hideGizmo = false; // Hides the gizmo if it's disabled
        public bool noMessage = false; // Makes it so the message never appears

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
        public List<SkillLevel> targetSkillLimiters;
        public List<StatCheck> targetStatLimiters;
        public List<NeedLevel> targetNeedLevels;
        public List<PawnKindDef> targetPawnKinds;
        public FloatRange bodySize = new FloatRange(0, 999);
        public bool invertBodySize = false;
        public TargetGroup targetGroup = TargetGroup.None;
        public List<FactionDef> validTargetFactions;
        public List<FactionDef> forbiddenTargetFactions;
        public List<MentalStateDef> targetMentalStates = new List<MentalStateDef>();
        public CheckType targetMentalStateCheck = CheckType.Required;

        // Caster Hediffs
        public List<HediffDef> casterHasAnyOfHediffs;
        public List<HediffDef> casterHasAllOfHediffs;
        public List<HediffDef> casterHasNoneOfHediffs;

        // Caster Pawn Checks
        public List<CapCheck> casterCapLimiters;
        public List<SkillLevel> casterSkillLimiters;
        public List<StatCheck> casterStatLimiters;
        public List<NeedLevel> casterNeedLevels;
        public List<FactionDef> validCasterFactions;
        public List<FactionDef> forbiddenCasterFactions;

        // % Light
        public FloatRange targetLightLevel = FloatRange.ZeroToOne;
        public bool invertTargetLight = false;
        public FloatRange casterLightLevel = FloatRange.ZeroToOne;
        public bool invertCasterLight = false;

        // Roof Check
        public RoofCheck casterRoof = RoofCheck.NoCheck;
        public RoofCheck targetRoof = RoofCheck.NoCheck;

        // Time checks
        public FloatRange progressThroughDay = FloatRange.ZeroToOne;
        public bool invertTime = false;
        public List<Season> seasons;

        // Map Condition
        public bool checkRoofForRainSnowRate;
        public FloatRange rainRate = new FloatRange(0, 9999);
        public FloatRange snowRate = new FloatRange(0, 9999);
        public List<PlanetLayerDef> casterLayers = new List<PlanetLayerDef>();
        public CheckType casterLayerCheck = CheckType.Required;
        public List<PlanetLayerDef> targetLayers = new List<PlanetLayerDef>();
        public CheckType targetLayerCheck = CheckType.Required;
        public CheckType sameLayerCheck = CheckType.None;
        
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

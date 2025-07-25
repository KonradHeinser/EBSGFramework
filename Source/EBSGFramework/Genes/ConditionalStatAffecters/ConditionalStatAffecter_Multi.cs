using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Multi : ConditionalStatAffecter
    {
        public List<CapCheck> capLimiters;

        public List<NeedLevel> needLevels;

        public List<SkillLevel> skillLimiters;

        public List<HediffDef> anyOfHediffs;

        public List<HediffDef> allOfHediffs;

        public List<HediffDef> noneOfHediffs;

        public List<GeneDef> anyOfGenes;

        public List<GeneDef> allOfGenes;

        public List<GeneDef> noneOfGenes;

        public IntRange daysOfQuadrum = new IntRange(0, 60);

        public bool invertDOQ;

        public IntRange daysOfYear = new IntRange(0, 60);

        public bool invertDOY;

        public List<Season> seasons;

        public FloatRange progressThroughDay = FloatRange.ZeroToOne;

        public bool invertTime = false;

        public bool inPollution = false;

        public bool notInPollution = false;

        public FloatRange lightLevel = FloatRange.ZeroToOne;

        public bool invertLight = false;

        public FloatRange temps = new FloatRange(-9999, 9999);

        public bool invertTemp = false;

        public List<TerrainDef> terrains;

        public bool hateTerrains = false;

        public bool anyWater = false;

        public bool anyNonWater = false;

        public List<GameConditionDef> conditions;

        public bool forbiddenConditions = false;

        public bool checkRoof = true;

        public FloatRange rainRate = new FloatRange(0, 9999);

        public FloatRange snowRate = new FloatRange(0, 9999);

        public List<WeatherDef> weathers;

        public bool forbiddenWeathers = false;

        public bool defaultActive = true; // Only applies when the pawn isn't spawned, and should only be messed with if some things are being used that require a spawned pawn

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrLiteral();
            return "EBSG_CorrectConditions".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                if (!pawn.CapacityConditionsMet(capLimiters)) return false;
                if (!pawn.AllNeedLevelsMet(needLevels)) return false;
                if (!pawn.AllSkillLevelsMet(skillLimiters)) return false;
                if (!pawn.CheckHediffTrio(anyOfHediffs, allOfHediffs, noneOfHediffs)) return false;
                if (!pawn.CheckGeneTrio(anyOfGenes, allOfGenes, noneOfGenes)) return false;

                int doq = GenLocalDate.DayOfQuadrum(pawn);
                if (daysOfQuadrum.ValidValue(doq) == invertDOQ)
                    return false;

                int doy = GenLocalDate.DayOfYear(pawn);
                if (daysOfYear.ValidValue(doy) == invertDOY)
                    return false;

                if (!pawn.CheckSeason(seasons, defaultActive))
                    return false;

                if (progressThroughDay.ValidValue(GenLocalDate.DayPercent(pawn)) == invertTime)
                    return false;

                if (pawn.Spawned)
                {
                    Map map = pawn.Map;
                    IntVec3 position = pawn.Position;

                    if (inPollution && !position.IsPolluted(map)) return false;
                    if (notInPollution && position.IsPolluted(map)) return false;

                    var roofed = position.Roofed(pawn.Map);
                    if ((rainRate.min > 0 || snowRate.min > 0) && checkRoof && roofed) 
                        return false;

                    if (lightLevel.ValidValue(map.glowGrid.GroundGlowAt(position)) == invertLight) 
                            return false;

                    if (temps.ValidValue(position.GetTemperature(map)) == invertTemp)
                        return false;

                    if (!CheckWater(position, map))
                        if (!terrains.NullOrEmpty())
                        {
                            if (hateTerrains && terrains.Contains(position.GetTerrain(map))) return false;
                            if (!hateTerrains && !terrains.Contains(position.GetTerrain(map))) return false;
                        }
                        else return false;

                    if (!conditions.NullOrEmpty() && map.GameConditionManager != null)
                    {
                        bool flag = false; // Recording if any of the conditions on the list are active
                        GameConditionManager manager = map.GameConditionManager;
                        foreach (GameConditionDef condition in conditions)
                            if (manager.ConditionIsActive(condition))
                            {
                                flag = true;
                                break;
                            }
                        if (flag && forbiddenConditions) return false;
                        if (!flag && !forbiddenConditions) return false;
                    }

                    if (map.weatherManager != null)
                    {
                        WeatherManager weather = map.weatherManager;

                        if (!checkRoof || !roofed)
                        {
                            if (!rainRate.ValidValue(weather.RainRate))
                                return false;

                            if (!snowRate.ValidValue(weather.SnowRate))
                                return false;
                        }

                        if (!weathers.NullOrEmpty())
                            if (weathers.Contains(weather.curWeather))
                            {
                                if (forbiddenWeathers) return false;
                            }
                            else if (!forbiddenWeathers) return false;
                    }
                }
                else return defaultActive;

                return true;
            }

            return false;
        }

        private bool CheckWater(IntVec3 position, Map map)
        {
            if (!anyWater && !anyNonWater) // In this situation the terrain list is deciding 
            {
                if (terrains.NullOrEmpty()) return true;
                return false;
            }
            if (anyWater && anyNonWater) return true; // If it allows all water and non-water, then everything works. This shouldn't happen
            if (anyWater && position.GetTerrain(map).IsWater) return true;
            if (anyNonWater && !position.GetTerrain(map).IsWater) return true;
            return false;
        }
    }
}

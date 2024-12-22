using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Multi : ConditionalStatAffecter
    {
        public List<CapCheck> capLimiters;

        public List<NeedLevel> needLevels;

        public List<SkillCheck> skillLimiters;

        public List<HediffDef> anyOfHediffs;

        public List<HediffDef> allOfHediffs;

        public List<HediffDef> noneOfHediffs;

        public List<GeneDef> anyOfGenes;

        public List<GeneDef> allOfGenes;

        public List<GeneDef> noneOfGenes;

        public float minPartOfDay = 0f;

        public float maxPartOfDay = 1f;

        public bool inPollution = false;

        public bool notInPollution = false;

        public float minLightLevel = 0f;

        public float maxLightLevel = 1f;

        public float minTemp = -9999f;

        public float maxTemp = 9999f;

        public List<TerrainDef> terrains;

        public bool hateTerrains = false;

        public bool anyWater = false;

        public bool anyNonWater = false;

        public List<GameConditionDef> conditions;

        public bool forbiddenConditions = false;

        public bool checkRoof = true;

        public float minimumRainRate = 0f;

        public float maximumRainRate = 9999f;

        public float minimumSnowRate = 0f;

        public float maximumSnowRate = 9999f;

        public List<WeatherDef> weathers;

        public bool forbiddenWeathers = false;

        public bool defaultActive = true; // Only applies when the pawn isn't spawned, and should only be messed with if some things are being used that require a spawned pawn

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectConditions".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
            {
                if (!req.Pawn.CapacityConditionsMet(capLimiters)) return false;
                if (!req.Pawn.AllNeedLevelsMet(needLevels)) return false;
                if (!req.Pawn.AllSkillLevelsMet(skillLimiters)) return false;
                if (!req.Pawn.CheckHediffTrio(anyOfHediffs, allOfHediffs, noneOfHediffs)) return false;
                if (!req.Pawn.CheckGeneTrio(anyOfGenes, allOfGenes, noneOfGenes)) return false;

                if (minPartOfDay > 0 || maxPartOfDay < 1)
                {
                    float time = GenLocalDate.DayPercent(req.Pawn);
                    if (time < minPartOfDay && time > maxPartOfDay) return false;
                }

                if (req.Pawn.Spawned)
                {
                    Map map = req.Pawn.Map;
                    IntVec3 position = req.Pawn.Position;

                    if (inPollution && !position.IsPolluted(map)) return false;
                    if (notInPollution && position.IsPolluted(map)) return false;
                    if ((minimumRainRate > 0 || minimumSnowRate > 0) && checkRoof && position.Roofed(map)) return false;

                    if (minLightLevel > 0 || maxLightLevel < 1)
                    {
                        float light = map.glowGrid.GroundGlowAt(position);
                        if (light < minLightLevel || light > maxLightLevel)
                            return false;
                    }

                    if (minTemp != -9999f || maxTemp != 9999f)
                    {
                        float temp = position.GetTemperature(map);
                        if (temp < minTemp && temp > maxTemp) return false;
                    }

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
                        if ((minimumRainRate > weather.RainRate) || (minimumSnowRate > weather.SnowRate) ||
                            (maximumRainRate < weather.RainRate && (!checkRoof || !position.Roofed(map))) ||
                            (maximumSnowRate < weather.SnowRate && (!checkRoof || !position.Roofed(map))))
                            return false;

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

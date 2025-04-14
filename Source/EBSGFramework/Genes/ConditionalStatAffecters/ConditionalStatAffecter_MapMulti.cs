using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_MapMulti : ConditionalStatAffecter
    {
        public float minPartOfDay = 0f;

        public float maxPartOfDay = 1f;

        public FloatRange progressThroughDay = FloatRange.ZeroToOne;

        public bool inPollution = false;

        public bool notInPollution = false;

        public float minLightLevel = 0f;

        public float maxLightLevel = 1f;

        public FloatRange lightLevel = FloatRange.ZeroToOne;

        public float minTemp = -9999f;

        public float maxTemp = 9999f;

        public FloatRange temps = new FloatRange(-9999, 9999);

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

        public FloatRange rainRate = new FloatRange(0, 9999);

        public FloatRange snowRate = new FloatRange(0, 9999);

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
            if (req.Thing is Pawn pawn && pawn.Spawned)
            {
                if (minPartOfDay > 0 || maxPartOfDay < 1 || progressThroughDay != FloatRange.ZeroToOne)
                {
                    float time = GenLocalDate.DayPercent(pawn);
                    if (!progressThroughDay.Includes(time)) return false;
                    if (time < minPartOfDay && time > maxPartOfDay) return false;
                }

                Map map = pawn.Map;
                IntVec3 position = pawn.Position;

                if (inPollution && !position.IsPolluted(map)) return false;
                if (notInPollution && position.IsPolluted(map)) return false;

                var roofed = position.Roofed(pawn.Map);
                if ((minimumRainRate > 0 || minimumSnowRate > 0 || rainRate.min > 0 || snowRate.min > 0) 
                    && checkRoof && roofed) return false;

                if (minLightLevel > 0 || maxLightLevel < 1  || lightLevel != FloatRange.ZeroToOne)
                {
                    float light = map.glowGrid.GroundGlowAt(position);
                    if (!lightLevel.Includes(light)) return false;
                    if (light < minLightLevel || light > maxLightLevel)
                        return false;
                }

                if (minTemp != -9999f || maxTemp != 9999f || temps != new FloatRange(-9999, 9999))
                {
                    float temp = position.GetTemperature(map);
                    if (!temps.Includes(temp)) return false;
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
                    var rain = map.weatherManager.RainRate;
                    var snow = map.weatherManager.SnowRate;

                    if ((!checkRoof || !roofed) &&
                        (minimumRainRate > rain) || (minimumSnowRate > snow) ||
                        (maximumRainRate < rain) || (maximumSnowRate < snow))
                        return false;


                    if (!rainRate.ValidValue(rain) && (!checkRoof || !roofed))
                        return false;

                    if (!snowRate.ValidValue(snow) && (!checkRoof || !roofed))
                        return false;

                    if (weathers.Contains(weather.curWeather))
                    {
                        if (forbiddenWeathers) return false;
                    }
                    else if (!forbiddenWeathers) return false;
                }

                return true;
            }

            return defaultActive;
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

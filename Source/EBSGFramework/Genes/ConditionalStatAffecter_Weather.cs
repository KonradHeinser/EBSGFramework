﻿using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Weather : ConditionalStatAffecter
    {
        public List<WeatherDef> weathers;

        public bool forbiddenWeathers = false;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_MapWeather".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null && req.Pawn.Spawned && req.Pawn.Map.GameConditionManager != null)
            {
                if (forbiddenWeathers)
                    return !weathers.Contains(req.Pawn.Map.weatherManager.curWeather);
                return weathers.Contains(req.Pawn.Map.weatherManager.curWeather);
            }
            return defaultActive;
        }
    }
}
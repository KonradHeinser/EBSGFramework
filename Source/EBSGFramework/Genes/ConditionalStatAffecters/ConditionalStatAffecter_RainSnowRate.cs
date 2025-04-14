using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_RainSnowRate : ConditionalStatAffecter
    {
        public bool checkRoof = true;

        public float minimumRainRate = 0f;

        public float maximumRainRate = 9999f;

        public float minimumSnowRate = 0f;

        public float maximumSnowRate = 9999f;

        public FloatRange rainRate = new FloatRange(0, 9999);

        public FloatRange snowRate = new FloatRange(0, 9999);

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectRainSnow".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.Spawned && pawn.Map.weatherManager != null)
            {
                Map map = pawn.Map;

                // Handles roof checks
                var roofed = pawn.Position.Roofed(pawn.Map);
                if (checkRoof && roofed)
                    return minimumRainRate <= 0 && minimumSnowRate <= 0
                        && rainRate.min <= 0 && snowRate.min <= 0;

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
                
                return true;
            }
            return defaultActive;
        }
    }
}

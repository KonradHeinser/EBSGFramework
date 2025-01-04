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

                // Handles roof checks for the minimums
                if (checkRoof && pawn.Position.Roofed(pawn.Map))
                    return minimumRainRate <= 0 && minimumSnowRate <= 0;

                if ((minimumRainRate > map.weatherManager.RainRate) || (minimumSnowRate > map.weatherManager.SnowRate) ||
                    (maximumRainRate < map.weatherManager.RainRate && (!checkRoof || !pawn.Position.Roofed(map))) ||
                    (maximumSnowRate < map.weatherManager.SnowRate && (!checkRoof || !pawn.Position.Roofed(map))))
                    return false;

                return true;
            }
            return defaultActive;
        }
    }
}

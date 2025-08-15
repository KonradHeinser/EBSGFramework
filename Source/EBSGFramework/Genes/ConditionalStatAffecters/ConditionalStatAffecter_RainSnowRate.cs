using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_RainSnowRate : ConditionalStatAffecter
    {
        public bool checkRoof = true;

        public FloatRange rainRate = new FloatRange(0, 9999);

        public FloatRange snowRate = new FloatRange(0, 9999);

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrFormat();
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
                    return rainRate.min <= 0 && snowRate.min <= 0;

                if (!checkRoof || !roofed)
                    return rainRate.ValidValue(map.weatherManager.RainRate) && 
                        snowRate.ValidValue(map.weatherManager.SnowRate);
            }
            return defaultActive;
        }
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Temperature : ConditionalStatAffecter
    {
        public float minTemp = -9999f;

        public float maxTemp = 9999f;

        public FloatRange temps = new FloatRange(-9999, 9999);

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            if (temps != new FloatRange(-9999, 9999))
            {
                if (temps.min == -9999f) return "EBSG_BelowTemperature".Translate(temps.max);
                if (temps.max == 9999f) return "EBSG_AboveTemperature".Translate(temps.min);
                return "EBSG_CorrectTemperature".Translate(temps.min, temps.max);
            }
            if (minTemp == -9999f) return "EBSG_BelowTemperature".Translate(maxTemp);
            if (maxTemp == 9999f) return "EBSG_AboveTemperature".Translate(minTemp);
            return "EBSG_CorrectTemperature".Translate(minTemp, maxTemp);
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.Spawned)
            {
                float temp = pawn.Position.GetTemperature(pawn.Map);
                if (!temps.Includes(temp)) return false;
                return temp >= minTemp && temp <= maxTemp;
            }
            return defaultActive;
        }
    }
}

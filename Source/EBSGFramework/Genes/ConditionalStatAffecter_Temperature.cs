using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Temperature : ConditionalStatAffecter
    {
        public float minTemp = -9999f;

        public float maxTemp = 9999f;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            if (minTemp == -9999f) return "EBSG_BelowTemperature".Translate(maxTemp);
            if (maxTemp == 9999f) return "EBSG_AboveTemperature".Translate(minTemp);
            return "EBSG_CorrectTemperature".Translate(minTemp, maxTemp);
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null && req.Pawn.Spawned)
            {
                float temp = req.Pawn.Position.GetTemperature(req.Pawn.Map);
                return temp >= minTemp && temp <= maxTemp;
            }
            return defaultActive;
        }
    }
}

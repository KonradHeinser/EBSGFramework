using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Temperature : ConditionalStatAffecter
    {
        public float minTemp = -9999f;

        public float maxTemp = 9999f;

        public FloatRange temp = new FloatRange(-9999, 9999);

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            if (temp != new FloatRange(-9999, 9999))
            {
                if (temp.min == -9999f) return "EBSG_BelowTemperature".Translate(temp.max);
                if (temp.max == 9999f) return "EBSG_AboveTemperature".Translate(temp.min);
                return "EBSG_CorrectTemperature".Translate(temp.min, temp.max);
            }
            if (minTemp == -9999f) return "EBSG_BelowTemperature".Translate(maxTemp);
            if (maxTemp == 9999f) return "EBSG_AboveTemperature".Translate(minTemp);
            return "EBSG_CorrectTemperature".Translate(minTemp, maxTemp);
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.Spawned)
            {
                float t = pawn.Position.GetTemperature(pawn.Map);
                if (!temp.Includes(t)) return false;
                return t >= minTemp && t <= maxTemp;
            }
            return defaultActive;
        }
    }
}

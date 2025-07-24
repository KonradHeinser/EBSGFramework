using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Temperature : ConditionalStatAffecter
    {
        public FloatRange temp = new FloatRange(-9999, 9999);

        public bool invert = false;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            if (temp.min == -9999f) return "EBSG_BelowTemperature".Translate(temp.max);
            if (temp.max == 9999f) return "EBSG_AboveTemperature".Translate(temp.min);
            return "EBSG_CorrectTemperature".Translate(temp.min, temp.max);
        }
            

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.Spawned)
                return temp.ValidValue(pawn.Position.GetTemperature(pawn.Map)) != invert;
            return defaultActive;
        }
    }
}

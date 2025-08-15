using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_DayOfYear : ConditionalStatAffecter
    {
        public IntRange daysOfYear = new IntRange(0, 60);

        public bool invert;

        public string label = null;

        public override string Label => GetLabel();

        public bool defaultActive;

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrFormat();
            return "EBSG_DoY".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                int doy = GenLocalDate.DayOfYear(pawn);
                return daysOfYear.ValidValue(doy) != invert;
            }
            return defaultActive;
        }
    }
}

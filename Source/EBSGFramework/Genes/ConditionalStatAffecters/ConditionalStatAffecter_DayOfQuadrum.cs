using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_DayOfQuadrum : ConditionalStatAffecter
    {
        public IntRange daysOfQuadrum = new IntRange(0, 15);

        public bool invert;

        public string label = null;

        public override string Label => GetLabel();

        public bool defaultActive;

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrLiteral();
            return "EBSG_DoQ".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                int doq = GenLocalDate.DayOfQuadrum(pawn);
                return daysOfQuadrum.ValidValue(doq) != invert;
            }
            return defaultActive;
        }
    }
}

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_TimeMulti : ConditionalStatAffecter
    {
        public IntRange daysOfQuadrum = new IntRange(0, 60);

        public bool invertDOQ;

        public IntRange daysOfYear = new IntRange(0, 60);

        public bool invertDOY;

        public List<Season> seasons;

        public FloatRange progressThroughDay = FloatRange.ZeroToOne;

        public bool invertTime = false;

        public string label = null;

        public override string Label => GetLabel();

        public bool defaultActive;

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrLiteral();
            return "EBSG_CorrectConditions".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                int doq = GenLocalDate.DayOfQuadrum(pawn);
                if (daysOfQuadrum.ValidValue(doq) == invertDOQ)
                    return false;

                int doy = GenLocalDate.DayOfYear(pawn);
                if (daysOfYear.ValidValue(doy) == invertDOY)
                    return false;

                if (!pawn.CheckSeason(seasons, defaultActive))
                    return false;

                if (progressThroughDay.ValidValue(GenLocalDate.DayPercent(pawn)) == invertTime)
                    return false;

                return true;
            }
            return defaultActive;
        }
    }
}

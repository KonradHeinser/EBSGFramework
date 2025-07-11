﻿using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Time : ConditionalStatAffecter
    {
        public FloatRange progressThroughDay = FloatRange.ZeroToOne;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectTime".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
                return progressThroughDay.ValidValue(GenLocalDate.DayPercent(pawn));
            return false;
        }
    }
}

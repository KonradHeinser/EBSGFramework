using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Time : ConditionalStatAffecter
    {
        public float minPartOfDay = 0f;

        public float maxPartOfDay = 1f;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectTime".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
            {
                float time = GenLocalDate.DayPercent(req.Pawn);
                return time >= minPartOfDay && time <= maxPartOfDay;
            }
            return false;
        }
    }
}

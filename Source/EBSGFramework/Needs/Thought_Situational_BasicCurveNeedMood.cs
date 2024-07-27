using RimWorld;

namespace EBSGFramework
{
    public class Thought_Situational_BasicCurveNeedMood : Thought_Situational
    {
        private EBSGExtension extension;

        public EBSGExtension Extension
        {
            get
            {
                if (extension == null)
                    extension = def.GetModExtension<EBSGExtension>();
                return extension;
            }
        }

        public override float MoodOffset()
        {
            if (Extension == null || Extension.need == null || Extension.moodOffsetCurve == null || pawn.needs == null) return 0f;

            Need need = pawn.needs.TryGetNeed(Extension.need);
            if (need == null) return 0f;

            return Extension.moodOffsetCurve.Evaluate(need.CurLevel);
        }
    }
}

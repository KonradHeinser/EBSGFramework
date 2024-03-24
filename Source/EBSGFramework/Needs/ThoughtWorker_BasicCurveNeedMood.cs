using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_BasicCurveNeedMood : ThoughtWorker
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

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (Extension == null || Extension.need == null || Extension.moodOffsetCurve == null || p.needs == null) return ThoughtState.Inactive;
            Need need = p.needs.TryGetNeed(Extension.need);
            return need != null && Extension.moodOffsetCurve.Evaluate(need.CurLevel) != 0;
        }
    }
}

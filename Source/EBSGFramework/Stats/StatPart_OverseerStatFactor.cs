using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class StatPart_OverseerStatFactor : StatPart
    {
        private StatDef stat = null;

        [MustTranslate]
        public string label;

        public override string ExplanationPart(StatRequest req)
        {
            if (GetFactor(req, out var factor) && factor != 1)
                return $"{label} : x{factor.ToStringByStyle(stat.toStringStyle, stat.toStringNumberSense)}";
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (GetFactor(req, out var factor))
                val *= factor;
        }

        private bool GetFactor(StatRequest req, out float factor)
        {
            factor = 0f;

            if (ModsConfig.BiotechActive && req.HasThing && req.Thing is Pawn pawn)
            {
                Pawn overseer = pawn.GetOverseer();
                if (overseer != null)
                {
                    factor = overseer.StatOrOne(stat);
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class StatPart_OverseerStatOffset : StatPart
    {
        private StatDef stat = null;

        [MustTranslate]
        public string label;

        public override string ExplanationPart(StatRequest req)
        {
            if (GetOffset(req, out var offset) && offset != 0)
            {
                string sign = offset < 0 ? "-" : "+";
                return $"{label} : {sign}{Math.Abs(offset).ToStringByStyle(stat.toStringStyle, stat.toStringNumberSense)}";
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (GetOffset(req, out var offset))
                val += offset;
        }

        private bool GetOffset(StatRequest req, out float offset)
        {
            offset = 0f;

            if (ModsConfig.BiotechActive && req.HasThing && req.Thing is Pawn pawn)
            {
                Pawn overseer = pawn.GetOverseer();
                if (overseer != null)
                {
                    offset = overseer.StatOrOne(stat);
                    return true;
                }
            }
            return false;
        }
    }
}

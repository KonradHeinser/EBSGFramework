using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Light : ConditionalStatAffecter
    {
        public FloatRange lightLevel = FloatRange.ZeroToOne;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            if (lightLevel.min <= 0f) return "EBSG_BelowLight".Translate(lightLevel.max.ToStringPercent());
            if (lightLevel.max >= 1f) return "EBSG_AboveLight".Translate(lightLevel.min.ToStringPercent());
            return "EBSG_CorrectLight".Translate(lightLevel.min.ToStringPercent(), lightLevel.max.ToStringPercent());
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.Spawned)
                return lightLevel.ValidValue(pawn.Map.glowGrid.GroundGlowAt(pawn.Position));
            return defaultActive;
        }
    }
}

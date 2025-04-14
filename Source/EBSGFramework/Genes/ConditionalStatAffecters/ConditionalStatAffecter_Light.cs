using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Light : ConditionalStatAffecter
    {
        public float minLightLevel = 0f;

        public float maxLightLevel = 1f;

        public FloatRange lightLevel = FloatRange.ZeroToOne;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            if (lightLevel != FloatRange.ZeroToOne)
            {
                if (lightLevel.min <= 0f) return "EBSG_BelowLight".Translate(lightLevel.max.ToStringPercent());
                if (lightLevel.max >= 1f) return "EBSG_AboveLight".Translate(lightLevel.min.ToStringPercent());
                return "EBSG_CorrectLight".Translate(lightLevel.min.ToStringPercent(), lightLevel.max.ToStringPercent());
            }
            if (minLightLevel <= 0f) return "EBSG_BelowLight".Translate(maxLightLevel.ToStringPercent());
            if (maxLightLevel >= 1f) return "EBSG_AboveLight".Translate(minLightLevel.ToStringPercent());
            return "EBSG_CorrectLight".Translate(minLightLevel.ToStringPercent(), maxLightLevel.ToStringPercent());
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.Spawned)
            {
                float light = pawn.Map.glowGrid.GroundGlowAt(pawn.Position);
                if (!lightLevel.Includes(light)) return false;
                if (light < minLightLevel || light > maxLightLevel)
                {
                    return false;
                }
                return true;
            }
            return defaultActive;
        }
    }
}

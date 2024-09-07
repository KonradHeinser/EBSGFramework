using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Light : ConditionalStatAffecter
    {
        public float minLightLevel = 0f;

        public float maxLightLevel = 1f;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            if (minLightLevel <= 0f) return "EBSG_BelowLight".Translate(maxLightLevel.ToStringPercent());
            if (maxLightLevel >= 1f) return "EBSG_AboveLight".Translate(minLightLevel.ToStringPercent());
            return "EBSG_CorrectLight".Translate(minLightLevel.ToStringPercent(), maxLightLevel.ToStringPercent());
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null && req.Pawn.Spawned)
            {
                float light = req.Pawn.Map.glowGrid.GroundGlowAt(req.Pawn.Position);
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

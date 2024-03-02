using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByLightLevel : HediffComp
    {
        public HediffCompProperties_SeverityByLightLevel Props => (HediffCompProperties_SeverityByLightLevel)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.IsHashIntervalTick(200)) return;
            Pawn pawn = parent.pawn;

            if (pawn.Map != null)
            {
                parent.Severity = Props.lightToSeverityCurve.Evaluate(pawn.Map.glowGrid.GameGlowAt(pawn.Position, false));
            }
            else if (Props.timeToSeverityCurve != null)
            {
                parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));
            }
        }
    }
}

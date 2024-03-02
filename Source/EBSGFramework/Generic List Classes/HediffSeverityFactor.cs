using Verse;

namespace EBSGFramework
{
    public class HediffSeverityFactor
    {
        public HediffDef hediff;
        public float factor = 1f; // Multiplier to the hediff's severity that affects how much is added to the total. A negative factor will lower the resulting severity
        public float minResult = 0f; // The lowest result that can be given. If the factor is negative, this is the highest value added instead. If the pawn doesn't have the hediff, this is used
    }
}

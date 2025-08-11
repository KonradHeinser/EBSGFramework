using Verse;

namespace EBSGFramework
{
    public class HediffComp_CapacityFactor : HediffComp
    {
        public HediffCompProperties_CapacityFactor Props => props as HediffCompProperties_CapacityFactor;

        public float Factor(PawnCapacityDef cap)
        {
            float num = 1;

            if (!Props.validSeverity.ValidValue(parent.Severity))
                return num;

            foreach (var factor in Props.capMods)
                if (factor.capacity == cap)
                {
                    float num2 = factor.factor;
                    if (factor.stat != null)
                    {
                        float num3 = Pawn.StatOrOne(factor.stat);
                        if (factor.statCurve != null)
                            num3 = factor.statCurve.Evaluate(num3);
                        num2 *= num3;
                    }

                    if (factor.multiplyFactorBySeverity)
                    {
                        float num4 = parent.Severity;
                        if (factor.severityCurve != null)
                            num4 = factor.severityCurve.Evaluate(num4);
                        num2 *= num4;
                    }

                    if (factor.curve != null)
                        num2 = factor.curve.Evaluate(num2);

                    num *= num2;
                    // There is no break here in case someone multiplies the same capacity by multiple stats
                }

            return num;
        }
    }
}

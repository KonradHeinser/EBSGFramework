using RimWorld;

namespace EBSGFramework
{
    public class Thought_Situational_HediffSeverity : Thought_Situational
    {
        public override float MoodOffset()
        {
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();

            if (thoughtExtension?.hediff != null && thoughtExtension.curve != null)
            {
                if (pawn.HasHediff(thoughtExtension.hediff, out var hediff))
                    return thoughtExtension.curve.Evaluate(hediff.Severity);
            }

            return base.MoodOffset();
        }
    }
}

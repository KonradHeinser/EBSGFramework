using Verse;

namespace EBSGFramework
{
    public class Hediff_ResourceCraving : HediffWithComps
    {
        public override string SeverityLabel
        {
            get
            {
                if (Severity == 0f)
                {
                    return null;
                }
                return Severity.ToStringPercent();
            }
        }
    }
}

using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_DisplaySeverity : HediffCompProperties
    {
        public string prependString; // Text before

        public float postFactor = 1; // In case you want to display a different number that's just based on the hediff severity

        public int roundDigits = 3; // How many numbers past the decimal that can be displayed

        public string appendString; // Text after

        public HediffCompProperties_DisplaySeverity()
        {
            compClass = typeof(HediffComp_DisplaySeverity);
        }
    }
}

using System;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_DisplaySeverity : HediffComp
    {
        public HediffCompProperties_DisplaySeverity Props => (HediffCompProperties_DisplaySeverity)props;

        public override string CompTipStringExtra
        {
            get
            {
                string output = "\n";
                if (Props.prependString != null) output += Props.prependString;

                output += Math.Round((decimal)(parent.Severity * Props.postFactor), Props.roundDigits).ToString();

                if (Props.appendString != null) output += Props.appendString;

                return output;
            }
        }
    }
}

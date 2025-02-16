using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class AgingXenotypeExtension : DefModExtension
    {
        public List<XenoRange> xenotypes;

        public ThingDef filth;

        public IntRange filthAmount = new IntRange(4, 7);

        public bool setXenotype = true;

        public string message;
    }
}

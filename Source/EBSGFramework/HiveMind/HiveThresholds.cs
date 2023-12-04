using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HiveThresholds
    {
        public int maxCount = 0;
        public HediffDef hediffWhenTooMany;
        public List<HediffDef> hediffsWhenTooMany;
        public int minCount = 0;
        public HediffDef hediffWhenTooFew;
        public List<HediffDef> hediffsWhenTooFew;
    }
}

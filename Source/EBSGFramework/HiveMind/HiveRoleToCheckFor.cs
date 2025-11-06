using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HiveRoleToCheckFor
    {
        public string checkKey = "Core"; // This can be the same as key if there is supposed to be a maximum or minimum count of this gene

        public int minCount = 0;
        public HediffDef hediffWhenTooFew = null;
        public List<HediffDef> hediffsWhenTooFew = null;

        public HediffDef hediffWhenEnough= null; // If over min and under a viable max, this is applied
        public List<HediffDef> hediffsWhenEnough = null;

        public int maxCount = 0;
        public HediffDef hediffWhenTooMany = null;
        public List<HediffDef> hediffsWhenTooMany = null;
    }
}

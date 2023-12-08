using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HiveRoleToCheckFor
    {
        public string checkKey; // This can be the same as key if there is supposed to be a maximum or minimum count of this gene

        public int maxCount = 0;
        public HediffDef hediffWhenTooMany;
        public List<HediffDef> hediffsWhenTooMany;

        public int minCount = 0;
        public HediffDef hediffWhenTooFew;
        public List<HediffDef> hediffsWhenTooFew;
    }
}

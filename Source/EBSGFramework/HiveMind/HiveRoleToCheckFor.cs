using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HiveRoleToCheckFor
    {
        public string checkKey; // This can be the same as key if there is supposed to be a maximum or minimum count of this gene
        public List<HiveThreshold> thresholds;
    }
}

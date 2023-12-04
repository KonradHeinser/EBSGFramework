using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HiveRolesToCheckFor
    {
        public string checkKey; // This can be the same as key if there is supposed to be a maximum or minimum count of this gene
        public List<HiveThresholds> thresholds;
    }
}

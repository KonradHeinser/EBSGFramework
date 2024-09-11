using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Capacities : ConditionalStatAffecter
    {
        public List<CapCheck> capLimiters;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectCapacities".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
            {
                foreach (CapCheck capCheck in capLimiters)
                {
                    if (!req.Pawn.health.capacities.CapableOf(capCheck.capacity))
                    {
                        if (capCheck.minCapValue > 0)
                            return false;
                        continue;
                    }

                    float capValue = req.Pawn.health.capacities.GetLevel(capCheck.capacity);
                    if (capValue < capCheck.minCapValue)
                        return false;
                    if (capValue > capCheck.maxCapValue)
                        return false;
                }
                return true;
            }

            return false;
        }
    }
}

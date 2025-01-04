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
            if (req.Thing is Pawn pawn)
                return pawn.CapacityConditionsMet(capLimiters);

            return false;
        }
    }
}

using Verse;
using Verse.AI;
using System.Collections.Generic;
using RimWorld;
using System.Linq;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalCurrentRainRate : ThinkNode_Conditional
    {
        public float rainRate = 1f;

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.Map == null) return false;

            return pawn.Map.weatherManager.RainRate >= rainRate;
        }
    }
}

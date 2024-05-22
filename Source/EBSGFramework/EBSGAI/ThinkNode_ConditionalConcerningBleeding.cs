using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalConcerningBleeding : ThinkNode_Conditional
    {
        private float bleedThreshold = 0.01f;

        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.health.hediffSet.BleedRateTotal > bleedThreshold;
        }
    }
}
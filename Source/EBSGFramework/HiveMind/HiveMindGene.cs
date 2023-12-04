using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HiveMindGene : Gene
    {
        public override void Tick()
        {
            if (!pawn.IsHashIntervalTick(200)) return; // To avoid performance issues from constant checking
        }
    }
}

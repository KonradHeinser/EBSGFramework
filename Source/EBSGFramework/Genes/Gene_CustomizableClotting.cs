using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class Gene_CustomizableClotting : HediffAdder
    {
        public override void Tick()
        {
            base.Tick();
            if (Extension == null || !pawn.IsHashIntervalTick(Extension.clotCheckInterval) || !pawn.health.HasHediffsNeedingTend()) return;

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int num = hediffs.Count - 1; num >= 0; num--)
                if (hediffs[num].Bleeding)
                    hediffs[num].Tended(Extension.tendQuality.RandomInRange, Extension.tendQuality.TrueMax, 1);
        }
    }
}

using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Gene_CustomizableClotting : HediffAdder
    {
        public override void Tick()
        {
            base.Tick();
            if (Extension == null || !pawn.IsHashIntervalTick(Extension.clotCheckInterval) || !pawn.health.HasHediffsNeedingTend()) return;

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            if (Extension.minTendQuality < 0) Extension.minTendQuality = 0;
            if (Extension.maxTendQuality > 1) Extension.maxTendQuality = 1;
            FloatRange TendingQualityRange = new FloatRange(Extension.minTendQuality, Extension.maxTendQuality);
            for (int num = hediffs.Count - 1; num >= 0; num--)
                if (hediffs[num].Bleeding)
                    hediffs[num].Tended(TendingQualityRange.RandomInRange, TendingQualityRange.TrueMax, 1);
        }
    }
}

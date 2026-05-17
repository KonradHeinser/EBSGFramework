using Verse;

namespace EBSGFramework
{
    public class HediffComp_NeedOffsetPerHour: HediffComp
    {
        private HediffCompProperties_NeedOffsetPerHour Props => (HediffCompProperties_NeedOffsetPerHour)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!Pawn.IsHashIntervalTick(30))
                return;
            
            var offset = Props.needOffsets.FirstOrDefault(n => n.threshold.ValidValue(parent.Severity));
            Pawn.HandleNeedOffsets(offset?.needOffsets, hashInterval:30, hourlyRate:true, factor:(Props.multiplyBySeverity ? parent.Severity : 1));
        }
    }
}
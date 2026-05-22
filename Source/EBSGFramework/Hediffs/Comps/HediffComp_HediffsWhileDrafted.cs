using Verse;

namespace EBSGFramework
{
    public class HediffComp_HediffsWhileDrafted : HediffComp
    {
        private HediffCompProperties_HediffsWhileDrafted Props => (HediffCompProperties_HediffsWhileDrafted)props;
        
        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (!Pawn.IsHashIntervalTick(30, delta))
                return;
            
            // Adds/Refreshes the required hediffs, then makes sure the other hediffs are removed
            Pawn.AddHediffToParts(Pawn.Drafted ? Props.draftedHediffs : Props.undraftedHediffs, refresh:true);
            Pawn.RemoveHediffsFromParts(Pawn.Drafted ? Props.undraftedHediffs : Props.draftedHediffs);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            // Makes sure all of the hediffs are removed
            Pawn.RemoveHediffsFromParts(Props.draftedHediffs);
            Pawn.RemoveHediffsFromParts(Props.undraftedHediffs);
        }
    }
}
using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_HediffWhileInSeverityRange : HediffComp
    {
        private HediffCompProperties_HediffWhileInSeverityRange Props => (HediffCompProperties_HediffWhileInSeverityRange)props;

        public List<HediffDef> addedHediffs;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            CheckHediffs();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(200,delta)) 
                CheckHediffs();
        }

        protected void CheckHediffs()
        {
            if (addedHediffs == null) addedHediffs = new List<HediffDef>();
            foreach (var severityLevel in Props.hediffsAtSeverities)
            {
                if (severityLevel.range.ValidValue(parent.Severity))
                {
                    if (!addedHediffs.Contains(severityLevel.hediff) && !Pawn.HasHediff(severityLevel.hediff))
                    {
                        Pawn.AddOrAppendHediffs(hediff: severityLevel.hediff);
                        addedHediffs.Add(severityLevel.hediff);
                    }
                }
                else if (addedHediffs.Contains(severityLevel.hediff))
                {
                    Pawn.RemoveHediffs(severityLevel.hediff);
                    addedHediffs.Remove(severityLevel.hediff);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (addedHediffs != null && !addedHediffs.NullOrEmpty())
            {
                Pawn.RemoveHediffs(null, addedHediffs);
                addedHediffs.Clear();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref addedHediffs, "EBSG_hediffAddedHediffs");
        }
    }
}

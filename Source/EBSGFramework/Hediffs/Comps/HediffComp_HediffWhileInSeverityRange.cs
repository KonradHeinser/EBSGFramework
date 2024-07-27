using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_HediffWhileInSeverityRange : HediffComp
    {
        private HediffCompProperties_HediffWhileInSeverityRange Props => (HediffCompProperties_HediffWhileInSeverityRange)props;

        public List<HediffDef> addedHediffs;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (Props.hediffsAtSeverities.NullOrEmpty())
            {
                Log.Error(Def + " doesn't have a set hediffsAtSeverities in HediffCompProperties_HediffWhileInSeverityRange. Removing hediff to avoid more errors.");
                Pawn.health.RemoveHediff(parent);
            }
            CheckHediffs();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(200)) CheckHediffs();
        }

        public void CheckHediffs()
        {
            if (addedHediffs == null) addedHediffs = new List<HediffDef>();
            foreach (HediffSeverityLevel severityLevel in Props.hediffsAtSeverities)
            {
                if (parent.Severity >= severityLevel.minSeverity && parent.Severity <= severityLevel.maxSeverity)
                {
                    if (!addedHediffs.Contains(severityLevel.hediff) && !EBSGUtilities.HasHediff(Pawn, severityLevel.hediff))
                    {
                        EBSGUtilities.AddOrAppendHediffs(Pawn, hediff: severityLevel.hediff);
                        addedHediffs.Add(severityLevel.hediff);
                    }
                }
                else
                {
                    if (addedHediffs.Contains(severityLevel.hediff) && EBSGUtilities.HasHediff(Pawn, severityLevel.hediff))
                    {
                        EBSGUtilities.RemoveHediffs(Pawn, severityLevel.hediff);
                        addedHediffs.Remove(severityLevel.hediff);
                    }
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (addedHediffs != null && !addedHediffs.NullOrEmpty())
            {
                EBSGUtilities.RemoveHediffs(Pawn, null, addedHediffs);
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

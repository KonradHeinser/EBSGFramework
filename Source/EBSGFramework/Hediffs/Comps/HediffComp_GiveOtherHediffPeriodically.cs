using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_GiveOtherHediffPeriodically : HediffComp
    {
        public HediffCompProperties_GiveOtherHediffPeriodically Props => (HediffCompProperties_GiveOtherHediffPeriodically)props;


        private float SeverityChange
        {
            get
            {
                float sc = Props.severity.RandomInRange;
                if (Props.multiplySeverityByParentSeverity)
                    sc *= parent.Severity;
                switch (Props.severityCondition)
                {
                    case GiveSeverityCheck.Positive:
                        if (sc < 0)
                            return 0;
                        break;
                    case GiveSeverityCheck.Negative:
                        if (sc > 0)
                            return 0;
                        break;
                    default: break;
                }
                return sc;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(Props.interval) &&
                EBSGUtilities.WithinSeverityRanges(parent.Severity, Props.validSeverities) &&
                Rand.Chance(Props.chance))
            {
                // If the pick was 0 or didn't meet the condition, no need to do anything else
                var change = SeverityChange; 
                if (change == 0)
                    return;
                Pawn other = null;

                if (Props.linkingHediff != null)
                {
                    if (Pawn.HasHediff(Props.linkingHediff, out Hediff h) &&
                        h is HediffWithTarget targeter && targeter.target is Pawn t)
                        other = t;
                    else if (Props.linkMandatory)
                        return;
                }

                if (!Props.bodyParts.NullOrEmpty())
                {
                    Dictionary<BodyPartDef, int> foundParts = new Dictionary<BodyPartDef, int>();

                    foreach (BodyPartDef bodyPartDef in Props.bodyParts)
                    {
                        if (Pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).NullOrEmpty()) continue;
                        if (foundParts.NullOrEmpty() || !foundParts.ContainsKey(bodyPartDef))
                            foundParts.Add(bodyPartDef, 0);

                        Pawn.AddHediffToPart(Pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], Props.hediff, change, change, Props.onlyIfNew, other);
                        foundParts[bodyPartDef]++;
                    }
                }
                else
                    Pawn.AddOrAppendHediffs(change, Props.onlyIfNew ? 0 : change, 
                        Props.hediff, null, other);
            }
        }
    }
}

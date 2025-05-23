﻿using System.Collections.Generic;
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
                float sc = Props.severity;
                if (Props.multiplySeverityByParentSeverity)
                    sc *= parent.Severity;
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

                        Pawn.AddHediffToPart(Pawn.RaceProps.body.GetPartsWithDef(bodyPartDef).ToArray()[foundParts[bodyPartDef]], Props.hediff, SeverityChange, SeverityChange, Props.onlyIfNew, other);
                        foundParts[bodyPartDef]++;
                    }
                }
                else
                    Pawn.AddOrAppendHediffs(SeverityChange, Props.onlyIfNew ? 0 : SeverityChange, 
                        Props.hediff, null, other);
            }
        }
    }
}

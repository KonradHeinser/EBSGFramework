using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_GiveOtherHediffPeriodically : HediffCompProperties
    {
        public int interval = 2500;

        public HediffDef hediff;

        public FloatRange severity = FloatRange.One;

        public GiveSeverityCheck severityCondition = GiveSeverityCheck.None;

        public bool multiplySeverityByParentSeverity = false;

        public bool onlyIfNew = false;

        public List<BodyPartDef> bodyParts;

        public FloatRange validSeverities = FloatRange.Zero;

        public float chance = 1f;

        public HediffDef linkingHediff;

        public bool linkMandatory = true;

        public HediffCompProperties_GiveOtherHediffPeriodically()
        {
            compClass = typeof(HediffComp_GiveOtherHediffPeriodically);
        }
    }
}

﻿using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffsAtSeverities
    {
        public List<HediffDef> hediffDefs; // If multiple hediffs should be added in a set you can use this
        public HediffDef hediffDef; // If only one hediff should be added, or hediffs cascade based on severity without disappearing at higher levels, use this
        public BodyPartDef bodyPart; // If null applies to whole body
        public float initialSeverity = 1; // If the hediff doesn't exist on the pawn, it uses this
        public float severityPerDay = 0; // By default increases by 1 each day. This can be made negative to start removing hediffs that normally don't go down
        public FloatRange validSeverity = FloatRange.Zero;
    }
}

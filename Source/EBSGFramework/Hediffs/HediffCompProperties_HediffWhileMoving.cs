﻿using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_HediffWhileMoving : HediffCompProperties
    {
        public HediffDef hediffWhileMoving;
        public List<HediffDef> hediffsWhileMoving;
        public float initialMovingSeverity = 1f;
        public float movingSeverityPerHour = 0f;

        public HediffDef hediffWhileNotMoving;
        public List<HediffDef> hediffsWhileNotMoving;
        public float initialNotMovingSeverity = 1f;
        public float notMovingSeverityPerHour = 0f;
        public HediffCompProperties_HediffWhileMoving()
        {
            compClass = typeof(HediffComp_HediffWhileMoving);
        }
    }
}

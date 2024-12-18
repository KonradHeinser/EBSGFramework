﻿using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_AttachMote : HediffCompProperties
    {
        public ThingDef moteDef;

        public Color color = new Color(1f, 1f, 1f);

        public bool brightnessBySeverity;

        public float staticBrightness = 1f;

        public bool displayWhileDowned = false;

        public bool rotateWithPawn = true;

        public HediffCompProperties_AttachMote()
        {
            compClass = typeof(HediffComp_AttachMote);
        }
    }
}

using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TieredRegeneration : HediffCompProperties
    {
        public List<RegenSet> regenSets;
        public bool healWhileRegrowing = false; // By default, regrowing parts stops normal regen completely
        public bool prioritizeHeal = false; // By default, missing part restoration is prioritized over generic healing
        public HediffCompProperties_TieredRegeneration()
        {
            compClass = typeof(HediffComp_TieredRegeneration);
        }
    }
}

using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public static class CollectExtensionData
    {
        static CollectExtensionData()
        {
            foreach (ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                EBSGExtension thoughtExtension = thoughtDef.GetModExtension<EBSGExtension>();
            }
        }
        public static SimpleCurve PeopleToMoodCurve;
    }
}

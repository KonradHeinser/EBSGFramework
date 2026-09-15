using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class SeasonLink
    {
        public List<Season> seasons;

        public float severity = 1;

        public SimpleCurve dayToSeverity = null;
    }
}
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffsToParts
    {
        public List<BodyPartDef> bodyParts;
        public HediffDef hediff;
        public bool onlyIfNew = false;
        public float severity = 0.5f;
        public float minAge = 0;
        public float maxAge = 9999f;
    }
}

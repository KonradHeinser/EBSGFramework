using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffToGive
    {
        public HediffDef hediffDef;

        public bool onlyBrain;

        public bool applyToSelf;

        public bool onlyApplyToSelf;

        public bool applyToTarget = true;

        public bool replaceExisting;

        public float severity = 1f;

        public bool psychic = false;
    }
}

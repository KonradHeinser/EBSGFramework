using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffToGive
    {
        public HediffDef hediffDef;

        public List<HediffDef> hediffDefs;

        public bool onlyBrain;

        public List<BodyPartDef> bodyParts;

        public bool applyToSelf;

        public bool onlyApplyToSelf;

        public bool applyToTarget = true;

        public bool replaceExisting;

        public bool skipExisting;

        public FloatRange severity = FloatRange.One;

        public bool psychic = false;
    }
}

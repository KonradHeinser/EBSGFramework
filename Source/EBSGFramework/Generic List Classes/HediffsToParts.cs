using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffToParts
    {
        public HediffDef hediff;

        public List<BodyPartDef> bodyParts;

        public bool onlyIfNew = false;

        public float severity = 0.5f;

        public int degree = 0; // Used for adding hediffs via traits

        public float chance = 1f;

        public FloatRange validAges = new FloatRange(0f, 9999f);

        public bool removeOnRemove = true; // When false, things that remove all items in a HediffToParts list will ignore this item

        public List<ThingDef> consumedThings; // Only used for the Gene extension version
    }
}

using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class PostLovinThingsExtension : DefModExtension
    {
        public List<GeneDef> partnerRequiresOneOf; // If the partner doesn't have one of these genes, then this stuff doesn't occur

        public Gender gender = Gender.None;

        public Gender partnerGender = Gender.None;

        public List<ThingDefCountClass> spawnThings;

        public List<HediffToParts> hediffsToApplySelf;

        public List<HediffToParts> hediffsToApplyPartner;

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);
    }
}

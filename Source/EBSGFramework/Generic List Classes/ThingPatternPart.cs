using Verse;
namespace EBSGFramework
{
    public class ThingPatternPart
    {
        public int count = 1;

        public ThingDef thing;

        public bool reservedForLargeThing = false; // Use this if making a large thing that needs more than one open tile

        public IntVec2 relativeLocation = new IntVec2(0, 0);

        public bool skipIfBlocked = false; // When false, the location being filled or unstandable blocks the ability. Leaving this as true allows for partial successes
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class SkillChange
    {
        public SkillDef skill; // If null, picks a random skill
        public int passionChange = 0; // Only planned out for basic passion levels, so if another mod adds new passion levels this may ignore them. Positive values will try to increment upwards, which, depending on whether the external passions changed the Rimworld increment method or not, may catch new passions properly. Negative values will not work with moded passions, and always result in None.
        public IntRange skillChange = IntRange.Zero; // A negative value decreases skill level
        public bool setPassion; // Tells the code a static passion is being set
        public Passion passion; // None, Minor, or Major. If another mod adds Passions to the vanilla passion list, those should also be usable
    }
}

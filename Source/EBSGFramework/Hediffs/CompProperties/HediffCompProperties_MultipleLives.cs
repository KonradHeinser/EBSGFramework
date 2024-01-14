using Verse;
namespace EBSGFramework
{
    public class HediffCompProperties_MultipleLives : HediffCompProperties
    {
        public int extraLives = 0; // If set to -666, then this becomes infinite lives. There is no way to get this value outside of manually setting it in the comp
        public float daysToRecoverLife = 0; // Ignored if using severity
        public bool useSeverityNotDays = false; // Ignores days to recover and bases lives on max severity. Requires max severity to be set

        public float hoursToRevive = 0; // If 0, revival is instant. Life recovery is paused while the pawn is reviving

        public bool includeProgressOnTooltip = true;
        public bool includeRemainingLivesOnTooltip = true;

        public HediffCompProperties_MultipleLives()
        {
            compClass = typeof(HediffComp_MultipleLives);
        }
    }
}

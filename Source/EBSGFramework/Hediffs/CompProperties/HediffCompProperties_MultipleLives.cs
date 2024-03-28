using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_MultipleLives : HediffCompProperties
    {
        public int extraLives = 0; // If set to -666, then this becomes infinite lives. There is no way to get this value outside of manually setting it in the comp
        public float daysToRecoverLife = 0; // Ignored if using severity
        public bool useSeverityNotDays = false; // Ignores days to recover and bases lives on max severity. Requires max severity to be set

        public float revivalChance = 1f; // Chance that the pawn can begin reviving. If this doesn't come to pass, then infinite lives become 0, or an extra life is expended (until 0)
        public bool onlyOneChance = false; // If the revival fails, just die without expending additional lives
        public bool deleteOnFailedRevive = false; // Deletes corpse if the pawn fails to revive due to chance or lack of lives
        public SoundDef extraDeathSound; // If the delete is active, then things similar to the HediffCompProperties_DestroyOnDeath stuff become available
        public ThingDef thingSpawn;
        public List<ThingDef> thingsToSpawn;

        public float hoursToRevive = 0; // If -1, revival is instant. Life recovery is paused while the pawn is reviving
        public FloatRange randomHoursToRevive; // Only used if hoursToRevive is not changed

        public bool needBrainToRevive = false; // Checks if the cause of death was brain or head removal

        public bool includeProgressOnTooltip = true;
        public bool includeRemainingLivesOnTooltip = true;

        public bool indestructibleWhileResurrecting = false;
        public bool removeAllInjuriesAfterRevival = false;

        public HediffCompProperties_MultipleLives()
        {
            compClass = typeof(HediffComp_MultipleLives);
        }
    }
}

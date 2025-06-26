using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityLaunchProjectileBurst : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;

        public int burstCount = 1;

        public int ticksBetweenShots = 5;

        public SoundDef firingSound;

        public bool immediateLaunch = true;

        public CompProperties_AbilityLaunchProjectileBurst()
        {
            compClass = typeof(CompAbilityEffect_LaunchProjectileBurst);
        }

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (burstCount < 1)
                yield return "burstCount must be 1 or higher";

            if (ticksBetweenShots < 1)
                yield return "ticksBetweenShots must be 1 or higher";
        }
    }
}

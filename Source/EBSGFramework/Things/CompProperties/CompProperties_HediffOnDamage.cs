using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_HediffOnDamage : CompProperties
    {
        public HediffDef givenHediff;

        // When set, hediff's severity will be adjusted by damage amount multiplied by this number
        public float? severityPerDamage;
        // If hediff should be applied to the damaged bodypart or to the whole body
        public bool applyToBodypart = false;

        // Whenever the comp triggers on ranged/explosive/melee damage
        public bool triggeredByRangedDamage = true;
        public bool triggeredByExplosions = true;
        public bool triggeredByMeleeDamage = true;

        // List of whitelisted DamageDefs. When set, DamageDefs that are not in this list won't be affected.
        public List<DamageDef> whitelistedDamageDefs;

        // List of blacklisted DamageDefs. When set, DamageDefs that are in this list won't be affected.
        public List<DamageDef> blacklistedDamageDefs;

        public CompProperties_HediffOnDamage()
        {
            compClass = typeof(CompHediffOnDamage);
        }
    }
}

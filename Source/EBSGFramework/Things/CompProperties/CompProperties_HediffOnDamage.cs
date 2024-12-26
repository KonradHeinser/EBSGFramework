using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_HediffOnDamage : CompProperties
    {
        public HediffDef givenHediff;

        public float severityPerDamage = 0f;
        public bool applyToBodypart = false;

        public float initialSeverity = 1f;
        public float severityIncrease = 1f;

        public bool triggeredByRangedDamage = true;
        public bool triggeredByExplosions = true;
        public bool triggeredByMeleeDamage = true;

        public List<DamageDef> validDamageDefs;
        public List<DamageDef> ignoredDamageDefs;

        public CompProperties_HediffOnDamage()
        {
            compClass = typeof(CompHediffOnDamage);
        }
    }
}

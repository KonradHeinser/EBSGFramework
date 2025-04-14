using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityChangeWhenDamaged : HediffCompProperties
    {
        public int cooldownTicks = 0;

        public float minSeverity = 0f;

        public float maxSeverity = 99999f;

        public FloatRange validSeverity = FloatRange.Zero;

        public float severityChange = 0f;

        public float severityChangeFactor = 0f;

        public List<DamageDef> validDamageDefs;

        public List<DamageDef> forbiddenDamageDefs;

        public HediffCompProperties_SeverityChangeWhenDamaged()
        {
            compClass = typeof(HediffComp_SeverityChangeWhenDamaged);
        }
    }
}

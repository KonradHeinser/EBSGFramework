using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThingCreationItem
    {
        public ThingDef thing;

        public int count = 1;

        public float chance = 1f;

        public ThingDef stuff;

        public QualityCategory quality = QualityCategory.Normal;

        public HediffDef linkingHediff;

        public bool requireLink = true;

        public float minSeverity = 0f;

        public float maxSeverity = 999f;

        public float weight = 1f;
    }
}

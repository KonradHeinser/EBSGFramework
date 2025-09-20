using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_FlipGenderPeriodically : HediffCompProperties
    {
        public IntRange interval = new IntRange(600);

        public bool revertPostRemove = false;

        public bool saveBeard = false;

        public bool flipPostAdd = true;

        public HediffCompProperties_FlipGenderPeriodically()
        {
            compClass = typeof(HediffComp_FlipGenderPeriodically);
        }
    }
}

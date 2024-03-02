using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_Aging : HediffCompProperties
    {
        public float ageChangePerHour; // Years per hour. Can be negative, or a really small value. A value of around 0.0006944 would double aging rate

        public float ageChangePerDay; // Years per day. Can be negative, or a really small value. A value of around 0.0.0166667 would double aging rate

        public float lowestAgeWhenNegative = 18f; // Set to this to avoid any weird child system bugs popping up

        public HediffCompProperties_Aging()
        {
            compClass = typeof(HediffComp_Aging);
        }
    }
}

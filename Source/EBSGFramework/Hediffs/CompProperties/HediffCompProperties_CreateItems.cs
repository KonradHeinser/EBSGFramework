using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_CreateItems : HediffCompProperties
    {
        public IntRange intervalTicks = new IntRange(2500, 2500);

        public List<List<ThingCreationItem>> intervalThings;

        public List<List<ThingCreationItem>> onDeathThings;

        public List<List<ThingCreationItem>> onRemovalThings;

        public List<List<ThingCreationItem>> onDeathOrRemovalThings;

        public HediffCompProperties_CreateItems()
        {
            compClass = typeof(HediffComp_CreateItems);
        }
    }
}

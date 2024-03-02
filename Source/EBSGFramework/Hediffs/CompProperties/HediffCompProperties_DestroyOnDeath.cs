using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_DestroyOnDeath : HediffCompProperties
    {
        public SoundDef extraDeathSound;

        public ThingDef thingSpawn;

        public List<ThingDef> thingsToSpawn;

        public HediffCompProperties_DestroyOnDeath()
        {
            compClass = typeof(HediffComp_DestroyOnDeath);
        }
    }
}

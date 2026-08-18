using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_FactionChangeOnDeath : HediffCompProperties
    {
        public List<FactionDef> factions;

        public bool changeToKillerFaction = true;

        public bool changeIdeoToPrimary;
        
        public HediffCompProperties_FactionChangeOnDeath()
        {
            compClass = typeof(HediffComp_FactionChangeOnDeath);
        }
    }
}
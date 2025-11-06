using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_TriggerGameCondition : CompProperties_AbilityEffect
    {
        public GameConditionDef gameCondition;

        public int ticks = 60000;

        public List<ConditionDuration> gameConditions;

        public bool onlyFirst = true; // If using the list, only go with the first valid option

        public bool skipExisting = true; // If onlyFirst is false, having this be true will allow a partial success where only some of the list becomes active

        public CompProperties_TriggerGameCondition()
        {
            compClass = typeof(CompAbilityEffect_TriggerGameCondition);
        }
    }
}

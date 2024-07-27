using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class AbilitiesAtSeverities
    {
        public List<AbilityDef> abilityDefs; // If multiple abilities should be added in a set you can use this
        public AbilityDef abilityDef; // If only one ability should be added, or abilities cascade based on severity without disappearing at higher levels, use this
        public float minSeverity = 0; // By default gives ability while hediff exists
        public float maxSeverity = 1; // By default removes ability at normal max hediff severity
    }
}

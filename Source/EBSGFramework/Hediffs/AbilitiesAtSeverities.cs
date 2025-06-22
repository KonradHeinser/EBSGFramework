using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class AbilitiesAtSeverities
    {
        public List<AbilityDef> abilityDefs; // If multiple abilities should be added in a set you can use this

        public AbilityDef abilityDef; // If only one ability should be added, or abilities cascade based on severity without disappearing at higher levels, use this

        public FloatRange validSeverity = FloatRange.Zero;
    }
}

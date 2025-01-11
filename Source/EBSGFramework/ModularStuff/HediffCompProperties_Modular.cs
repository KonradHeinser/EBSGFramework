using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_Modular : HediffCompProperties
    {
        // If the modules should be dropped when the hediff is removed
        public bool dropModulesOnRemoval = true;

        // List of open slots
        public List<ModuleSlot> slots = new List<ModuleSlot>();

        public HediffCompProperties_Modular()
        {
            compClass = typeof(HediffComp_Modular);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByEquipped : HediffCompProperties
    {
        public float severity; // Amount given if nothing is found to be equipped

        public List<ThingLink> equipment; // Checking weapons

        public List<StringLink> equipmentTags; // weapon tag checks

        public List<ThingLink> apparel; // I mean, iykyk
        
        public List<StringLink> apparelTags; // Apparel tag checks. That's right, apparel was checking apparel

        public EquippedCheck check = EquippedCheck.None;

        public HediffCompProperties_SeverityByEquipped()
        {
            compClass = typeof(HediffComp_SeverityByEquipped);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;
            
            if (check == EquippedCheck.None)
                yield return "No check specified";
        }
    }
}
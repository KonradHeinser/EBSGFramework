using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityChangeTraits : CompProperties_AbilityEffect
    {
        public List<TraitDegree> addedTraits = new List<TraitDegree>();
        
        public bool haveNoneOfAddedTraits = false;
        
        public List<TraitDegree> removedTraits = new List<TraitDegree>();
        
        public bool haveAnyOfRemovedTraits = false;
        
        public bool haveAllOfRemovedTraits = false;
        
        public bool applyOnSelf = false;

        public bool onlySelf = false;
        
        public SuccessChance successChance;

        public CompProperties_AbilityChangeTraits()
        {
            compClass = typeof(CompAbilityEffect_ChangeTraits);
        }
    }
}
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityDuplicatePawn : CompProperties_AbilityEffectWithDuration
    {
        public bool duplicateCaster = false;
        
        public IntRange count = IntRange.One;

        public bool keepName = true;
        
        public Gender? gender = null;

        public bool randomGender = false;

        public bool keepNameOnGenderChange = false;
        
        public List<HediffToGive> hediffsToGive;

        public bool useCasterFaction = true;

        public PawnRelationDef relation;

        public PawnRelationDef casterRelation;

        public CompProperties_AbilityDuplicatePawn()
        {
            compClass = typeof(CompAbilityEffect_DuplicatePawn);
        }
    }
}
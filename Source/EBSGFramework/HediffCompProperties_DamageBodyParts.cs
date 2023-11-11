using System;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_DamageBodyParts : HediffCompProperties
    {
        public List<BodyPartDef> bodyPartsToRemove;
        public List<PartToDamage> bodyPartsToDamage;

        public HediffCompProperties_DamageBodyParts()
        {
            compClass = typeof(HediffComp_DamageBodyParts);
        }
    }
}

using System;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class PartToDamage
    {
        public BodyPartDef bodyPart;
        public float damageAmount; // If looking to do something like just taking away half of a part's health, use damagePercentage instead
        public float damagePercentage = 0; // A percentage of the part's maximum health to remove. If 0, this is ignored. If not 0, it overrides damage amount. 0.5 would be 50% damage
    }
}

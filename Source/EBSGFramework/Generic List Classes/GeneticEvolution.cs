using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class GeneticEvolution
    {
        public GeneDef result;

        public Inheritance inheritable;

        public float chancePerCheck = 1f;

        public bool ignoreChanceDuringPostAdd = false;

        public string message;

        public MessageTypeDef messageType;

        public List<HediffWithRange> hasAnyOfHediff;

        public List<HediffWithRange> hasAllOfHediff;

        public List<HediffWithRange> hasNoneOfHediff;

        public List<GeneDef> hasAnyOfGene;

        public List<GeneDef> hasAllOfGene;

        public List<GeneDef> hasNoneOfGene;

        public FloatRange validAges = new FloatRange(0, 0);

        public List<SkillLevel> skillRequirements;
    }
}

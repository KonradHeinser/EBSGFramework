using System;
using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class RitualCompProperties
    {
        [TranslationHandle]
        public Type compClass;

        public ResearchProjectDef researchPrerequisite;

        public List<IngredientCount> additionalCosts = new List<IngredientCount>();

        [MustTranslate]
        public string label;

        public bool attemptDefault = true;

        public bool alwaysAttempt = false; // Allows modders to create new prerequisites

        public IEnumerable<string> ConfigErrors()
        {
            if (compClass == null)
                yield return "compClass is null";
        }

        public virtual bool Available(Map map, IntVec3 center, List<Pawn> participants)
        {
            return true;
        }
    }
}

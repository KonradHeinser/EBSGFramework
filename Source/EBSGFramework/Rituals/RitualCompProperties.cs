using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public IEnumerable<string> ConfigErrors()
        {
            if (compClass == null)
                yield return "compClass is null";
        }
    }
}

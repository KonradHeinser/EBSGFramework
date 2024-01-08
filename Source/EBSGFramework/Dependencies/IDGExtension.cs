using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class IDGExtension : DefModExtension
    {
        // Ingestible Dependency Genes are genes that add a dependency to any ingestible item. I name good
        public List<ThingDef> validThings;
        public List<ThingCategoryDef> validCategories;
        public HediffDef dependencyHediff;

        public string dependencyLabel; // Required if not using a chemical
        public string descriptionOverride; // If you don't like the default description added to the hediff tooltip, make your own here. You will not be able to reference things like pawn name and pronoun in this, so use generic theys if you use this
        public bool checkIngredients; // If true, then consuming items with registered ingredients that fall under one of the above lists counts as consuming

        // descriptionOverride Note: If the relevant hediff does not use the standard HediffCompProperties_SeverityPerDay, then the hediff will throw errors with the default description, making this required

        // Note for anyone trying to figure out the code behind all this, it uses the same concepts as the ChemicalDependency variants, it's just a lot more robust
    }
}

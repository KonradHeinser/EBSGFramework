using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class FertilityByGenderAgeExtension : DefModExtension
    {
        // Curves that can be added to a gene to alter a pawn's natural age multiplier
        // WARNING: If multiple genes have the fertility age factor, only the first one found will take effect. The additional factors are not subject to this limitation, but will not alter the basic age factor
        // WARNING 2: fertilityAgeFactor is always checked last, so if you have both a male and female factor, it will never be used
        public SimpleCurve maleFertilityAgeFactor;
        public SimpleCurve femaleFertilityAgeFactor;
        public SimpleCurve fertilityAgeFactor;
        public List<GeneDef> overridingGenes;
    }
}

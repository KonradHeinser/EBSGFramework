using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityAlterItem : CompProperties_AbilityEffect
    {
        public int hpRestore = 0;

        public float maxHPPercentRestore = 0;

        public bool uncode = false;

        public bool untaint = false;
        
        public bool increaseQuality = false;

        public QualityCategory maxQuality = QualityCategory.Legendary;
        
        public bool decreaseQuality = false;
        
        public QualityCategory minQuality = QualityCategory.Awful;
        
        public List<List<ThingDef>> stuffingChange = new List<List<ThingDef>>();
        
        public CompProperties_AbilityAlterItem()
        {
            compClass = typeof(CompAbilityEffect_AlterItem);
        }
    }
}
using Verse;
using VanillaGenesExpanded;
using AnimalBehaviours;
using System.Collections.Generic;
using System.Reflection;

namespace EBSGFramework
{
    public static class VFECompatabilityUtilities
    {
        public static FieldInfo asexualInterval = typeof(HediffCompProperties_AsexualReproduction).GetField("reproductionIntervalDays", BindingFlags.Instance | BindingFlags.NonPublic);

        public static ThingDef BloodType(Pawn pawn)
        {
            if (StaticCollectionsClass.bloodtype_gene_pawns.ContainsKey(pawn))
            {
                return StaticCollectionsClass.bloodtype_gene_pawns[pawn];
            }
            return pawn.RaceProps.BloodDef;
        }

        public static int GetDefaultAsexualRate(HediffCompProperties properties)
        {
            if (properties is HediffCompProperties_AsexualReproduction asexual)
                return asexual.reproductionIntervalDays;
            return -1;
        }

        public static void SetAsexualRates(List<HediffDef> hediffs, Dictionary<string, int> rates)
        {
            foreach (HediffDef hediff in hediffs)
                asexualInterval.SetValue(hediff.CompProps<HediffCompProperties_AsexualReproduction>(), rates[hediff.defName]);
        }
    }
}

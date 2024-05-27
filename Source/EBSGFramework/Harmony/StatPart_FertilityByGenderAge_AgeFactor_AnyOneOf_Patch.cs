using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    [HarmonyPatch(typeof(StatPart_FertilityByGenderAge), "AgeFactor")]
    public static class StatPart_FertilityByGenderAge_AgeFactor_AnyOneOf_Patch
    {
        [HarmonyPostfix]
        public static float Postfix(float __result, Pawn pawn)
        {
            // Checking humanlike stops a weird bug involving owned animals, and spawned stops wasting a bit of performance on something inconsequential
            if (pawn != null && pawn.RaceProps.Humanlike && pawn.Spawned && !pawn.genes.GenesListForReading.NullOrEmpty())
            {
                List<Gene> currentGenes = pawn.genes.GenesListForReading;
                foreach (Gene gene in currentGenes)
                {
                    if (gene.def.HasModExtension<FertilityByGenderAgeExtension>())
                    {
                        FertilityByGenderAgeExtension extension = gene.def.GetModExtension<FertilityByGenderAgeExtension>();
                        if (!extension.overridingGenes.NullOrEmpty())
                        {
                            foreach (GeneDef geneDef in extension.overridingGenes)
                            {
                                if (EBSGUtilities.HasRelatedGene(pawn, geneDef))
                                {
                                    extension = geneDef.GetModExtension<FertilityByGenderAgeExtension>();
                                    break;
                                }
                            }
                        }
                        if (extension.maleFertilityAgeFactor != null && pawn.gender == Gender.Male)
                        {
                            return extension.maleFertilityAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                        }
                        else if (extension.femaleFertilityAgeFactor != null && pawn.gender == Gender.Female)
                        {
                            return extension.femaleFertilityAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                        }
                        else if (extension.fertilityAgeFactor != null)
                        {
                            return extension.fertilityAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                        }
                    }
                }
            }
            return __result;
        }
    }
}

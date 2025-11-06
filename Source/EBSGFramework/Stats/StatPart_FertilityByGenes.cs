using System.Text;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class StatPart_FertilityByGenes : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || pawn.genes == null || pawn.genes.GetFirstGeneOfType<AdditionalFertilityByAge>() == null || !ModsConfig.BiotechActive)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder(32);
            Factor(pawn, stringBuilder);
            if (pawn.health.hediffSet.HasHediffPreventsPregnancy() && (CheckForMinimums(pawn) || CheckForMaximums(pawn)))
            {
                stringBuilder.AppendLine("\nThis pawn has fertility limiting genes that are not active due to sterilization.");
            }
            else
            {
                if (CheckForMinimums(pawn)) stringBuilder.AppendLine("\nAt least one gene stops fertility from going below a certain value.");
                if (CheckForMaximums(pawn)) stringBuilder.AppendLine("\nAt least one gene stops fertility from going above a certain value.");
            }
            return stringBuilder.ToString();
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (ModsConfig.BiotechActive && req.Thing is Pawn pawn && pawn.genes != null && pawn.genes.GetFirstGeneOfType<AdditionalFertilityByAge>() != null)
            {
                if (pawn.health.hediffSet.HasHediffPreventsPregnancy()) val = 0f;
                else
                {
                    val *= Factor(pawn);
                    val = Minimum(pawn, val);
                }
            }
        }

        public static float Factor(Pawn pawn, StringBuilder explanation = null)
        {
            float num = 1f;
            Pawn_GeneTracker genes = pawn.genes;
            if (genes != null)
            {
                foreach (Gene gene in genes.GenesListForReading)
                {
                    EBSGExtension extension = gene.def.GetModExtension<EBSGExtension>();
                    if (extension != null)
                    {
                        float tempNum = 1;
                        if (pawn.gender == Gender.Male && extension.maleFertilityAgeAdditionalFactor != null) tempNum *= extension.maleFertilityAgeAdditionalFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                        if (pawn.gender == Gender.Female && extension.femaleFertilityAgeAdditionalFactor != null) tempNum *= extension.femaleFertilityAgeAdditionalFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                        if (extension.fertilityAgeAdditionalFactor != null) tempNum *= extension.fertilityAgeAdditionalFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                        if (tempNum != 1) explanation?.AppendLine(gene.def.label + ": x" + tempNum.ToStringPercent());
                        num *= tempNum;
                    }
                }
            }

            if (num < 0) num = 0;
            return num;
        }

        public static float Minimum(Pawn pawn, float val)
        {
            Pawn_GeneTracker genes = pawn.genes;
            if (genes != null)
            {
                foreach (Gene gene in genes.GenesListForReading)
                {
                    EBSGExtension extension = gene.def.GetModExtension<EBSGExtension>();
                    if (extension != null)
                    {
                        if (pawn.gender == Gender.Male)
                        {
                            if (extension.minMaleFertilityByAgeFactor != null)
                            {
                                float minFertilityByAge = extension.minMaleFertilityByAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                                if (minFertilityByAge > val) val = minFertilityByAge;
                            }
                            if (extension.maxMaleFertilityByAgeFactor != null)
                            {
                                float maxFertilityByAge = extension.maxMaleFertilityByAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                                if (maxFertilityByAge < val) val = maxFertilityByAge;
                            }
                            if (val < extension.minMaleFertility) val = extension.minMaleFertility;
                            if (val > extension.maxMaleFertility) val = extension.maxMaleFertility;
                        }
                        else
                        {
                            if (extension.minFemaleFertilityByAgeFactor != null)
                            {
                                float minFertilityByAge = extension.minFemaleFertilityByAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                                if (minFertilityByAge > val) val = minFertilityByAge;
                            }
                            if (extension.maxFemaleFertilityByAgeFactor != null)
                            {
                                float maxFertilityByAge = extension.maxFemaleFertilityByAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                                if (maxFertilityByAge < val) val = maxFertilityByAge;
                            }
                            if (val < extension.minFemaleFertility) val = extension.minFemaleFertility;
                            if (val > extension.maxFemaleFertility) val = extension.maxFemaleFertility;
                        }
                        if (extension.minFertilityByAgeFactor != null)
                        {
                            float minFertilityByAge = extension.minFertilityByAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                            if (minFertilityByAge > val) val = minFertilityByAge;
                        }
                        if (extension.maxFertilityByAgeFactor != null)
                        {
                            float maxFertilityByAge = extension.maxFertilityByAgeFactor.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                            if (maxFertilityByAge < val) val = maxFertilityByAge;
                        }
                        if (val < extension.minFertility) val = extension.minFertility;
                        if (val > extension.maxFertility) val = extension.maxFertility;
                    }
                }
            }
            return val;
        }

        public static bool CheckForMinimums(Pawn pawn)
        {
            Pawn_GeneTracker genes = pawn.genes;
            if (genes != null)
            {
                foreach (Gene gene in genes.GenesListForReading)
                {
                    EBSGExtension extension = gene.def.GetModExtension<EBSGExtension>();
                    if (extension != null)
                    {
                        if (extension.minFertility != 0 || extension.minFertilityByAgeFactor != null) return true;
                        if (pawn.gender == Gender.Male && (extension.minMaleFertility != 0 || extension.minMaleFertilityByAgeFactor != null)) return true;
                        if (pawn.gender == Gender.Female && (extension.minFemaleFertility != 0 || extension.minFemaleFertilityByAgeFactor != null)) return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckForMaximums(Pawn pawn)
        {
            Pawn_GeneTracker genes = pawn.genes;
            if (genes != null)
            {
                foreach (Gene gene in genes.GenesListForReading)
                {
                    EBSGExtension extension = gene.def.GetModExtension<EBSGExtension>();
                    if (extension != null)
                    {
                        if (extension.maxFertility != 999999 || extension.maxFertilityByAgeFactor != null) return true;
                        if (pawn.gender == Gender.Male && (extension.maxMaleFertility != 999999 || extension.maxMaleFertilityByAgeFactor != null)) return true;
                        if (pawn.gender == Gender.Female && (extension.maxFemaleFertility != 999999 || extension.maxFemaleFertilityByAgeFactor != null)) return true;
                    }
                }
            }
            return false;
        }
    }
}

using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EBSGFramework
{
    public class Gene_EvolvingGene : HediffAdder
    {
        public override void PostAdd()
        {
            base.PostAdd();

            if (Extension?.geneticEvolutions.NullOrEmpty() != false)
            {
                Log.Error($"{def} does not have any geneticEvolutions set. If this is intentional, switch to the HediffAdder class.");
                pawn.genes.RemoveGene(this);
                return;
            }
            if (Extension.checkEvolutionsPostAdd)
                CheckEvolution(true);
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(2500))
                CheckEvolution();
        }

        public void CheckEvolution(bool postAdd = false)
        {
            foreach (var evo in Extension.geneticEvolutions)
            {
                if ((!postAdd || !evo.ignoreChanceDuringPostAdd) && 
                    !Rand.Chance(evo.chancePerCheck)) continue;

                if (evo.skipIfCarrierHasResult && pawn.HasRelatedGene(evo.result))
                    continue;

                if (!pawn.CheckGeneTrio(evo.hasAnyOfGene, evo.hasAllOfGene, evo.hasNoneOfGene))
                    continue;

                if (!pawn.CheckHediffTrio(evo.hasAnyOfHediff, evo.hasAllOfHediff, evo.hasNoneOfHediff))
                    continue;

                if (!pawn.AllSkillLevelsMet(evo.skillRequirements))
                    continue;

                if (evo.validAges != FloatRange.Zero && !evo.validAges.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                    continue;

                
                if ((pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony) && evo.message != null && (pawn.MapHeld != null || pawn.GetCaravan() != null))
                    Messages.Message(evo.message.TranslateOrLiteral(pawn.LabelShort, evo.result?.LabelCap, evo.result?.label), pawn, evo.messageType ?? MessageTypeDefOf.NeutralEvent);

                bool xenogene;
                switch (evo.inheritable)
                {
                    case Inheritance.Same:
                        xenogene = !pawn.genes.Endogenes.Contains(this);
                        break;
                    case Inheritance.Endo:
                        xenogene = false;
                        break;
                    default:
                        xenogene = true; 
                        break;
                }
                pawn.AddGenesToPawn(xenogene, null, evo.result);
                pawn.genes.RemoveGene(this);
                return;
            }
        }
    }
}

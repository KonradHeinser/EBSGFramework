using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
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
            CheckEvolution();
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(10000))
                CheckEvolution();
        }

        public void CheckEvolution()
        {
            foreach (var evo in Extension.geneticEvolutions)
            {
                if (!pawn.CheckGeneTrio(evo.hasAnyOfGene, evo.hasAllOfGene, evo.hasNoneOfGene))
                    continue;

                if (!pawn.CheckHediffTrio(evo.hasAnyOfHediff, evo.hasAllOfHediff, evo.hasNoneOfHediff))
                    continue;

                if (!pawn.AllSkillLevelsMet(evo.skillRequirements))
                    continue;

                if (evo.validAges != FloatRange.Zero && !evo.validAges.Includes(pawn.ageTracker.AgeBiologicalYearsFloat))
                    continue;

                if ((pawn.IsColonist || pawn.IsPrisonerOfColony) && evo.message != null)
                    Messages.Message(evo.message.TranslateOrLiteral(pawn.LabelShort, evo.result?.LabelCap, evo.result?.label), pawn, evo.messageType ?? MessageTypeDefOf.NeutralEvent);

                pawn.AddGenesToPawn(evo.xenogene, null, evo.result);
                pawn.genes.RemoveGene(this);
                return;
            }
        }
    }
}

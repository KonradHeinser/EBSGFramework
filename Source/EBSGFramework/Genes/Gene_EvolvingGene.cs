using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class Gene_EvolvingGene : HediffAdder
    {
        private int evolutionsRemaining = 1;

        public Gene_EvolvingGene parent;

        public override void PostAdd()
        {
            base.PostAdd();

            if (Extension?.geneticEvolutions.NullOrEmpty() != false)
            {
                Log.Error($"{def} does not have any geneticEvolutions set. If this is intentional, switch to the HediffAdder class.");
                pawn.genes.RemoveGene(this);
                return;
            }
            evolutionsRemaining = Extension.maxEvolutions;
            if (Active && Extension.checkEvolutionsPostAdd)
                CheckEvolution(true);
        }

        public override void Tick()
        {
            base.Tick();

            if (Active && pawn.IsHashIntervalTick(2500))
                CheckEvolution();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            parent?.ChildRemoved();
        }

        public void CheckEvolution(bool postAdd = false)
        {
            if (evolutionsRemaining == 0)
                return;

            foreach (var evo in Extension.geneticEvolutions)
            {
                if ((!postAdd || !evo.ignoreChanceDuringPostAdd) && !Rand.Chance(evo.chancePerCheck)) 
                    continue;

                if (!evo.validAges.ValidValue(pawn.ageTracker.AgeBiologicalYearsFloat))
                    continue;

                if ((evo.skipIfCarrierHasResult || Extension.maxEvolutions != 1) && pawn.HasRelatedGene(evo.result))
                    continue;

                if (!pawn.CheckGeneTrio(evo.hasAnyOfGene, evo.hasAllOfGene, evo.hasNoneOfGene))
                    continue;

                if (!pawn.CheckHediffTrio(evo.hasAnyOfHediff, evo.hasAllOfHediff, evo.hasNoneOfHediff))
                    continue;

                if (!pawn.AllSkillLevelsMet(evo.skillRequirements, false))
                    continue;

                if (!pawn.AllSkillLevelsInListMet(evo.complexSkillRequirements, false))
                    continue;

                if (evo.message != null && (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony) && (pawn.MapHeld != null || pawn.GetCaravan() != null))
                    Messages.Message(evo.message.TranslateOrFormat(pawn.LabelShort, evo.result?.LabelCap, evo.result?.label), pawn, evo.messageType ?? MessageTypeDefOf.NeutralEvent);
                
                if (evo.xenotypes?.NullOrEmpty() == false)
                    pawn.AlterXenotype(evo.xenotypes, null, IntRange.Zero, evo.setXenotype, false);

                if (evo.removeFirstGene)
                {
                    var g = pawn.GetFirstGeneFromList(evo.hasAnyOfGene);
                    if (g != null)
                        pawn.genes.RemoveGene(g);
                }

                if (evo.removeAllHasAllGene)
                    pawn.RemoveGenesFromPawn(evo.hasAllOfGene);
                
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
                
                switch (evo.addHasNoneCount)
                {
                    case 0:
                        break;
                    case -1:
                        pawn.AddGenesToPawn(xenogene, evo.hasNoneOfGene, null, this);
                        break;
                    default:
                        var options = new List<GeneDef>(evo.hasNoneOfGene);
                        options.Shuffle();
                        var addList = new List<GeneDef>();
                        var count = Mathf.Min(evo.addHasNoneCount, options.Count); // To make sure a modder can't mess things up and cause errors
                        for (int i = 0; i < count; i++)
                            addList.Add(options[i]);
                        pawn.AddGenesToPawn(xenogene, addList, null, this);
                        break;
                }
                
                pawn.AddGenesToPawn(xenogene, null, evo.result, parent: this);
                
                evolutionsRemaining--;
                if ((evo.result == null && !evo.dontRemove) || (evolutionsRemaining == 0 && Extension.keepEvolvingGene == evo.overrideKeep))
                    pawn.genes.RemoveGene(this);
                return;
            }
        }

        public void ChildRemoved()
        {
            if (Extension.recoverEvolutions)
                evolutionsRemaining++;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref evolutionsRemaining, "remaining", 1);
            Scribe_References.Look(ref parent, "parent");
        }
    }
}

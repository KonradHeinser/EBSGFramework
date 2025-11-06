using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_TreesDesiredNoPrecept : ThoughtWorker
    {   // Counters used by tree checker
        public int secondsSinceLastMultiSuper = 999999;
        public int secondsSinceLastSuper = 999999;
        public int secondsSinceLastFiveFull = 999999;
        public int secondsSinceLastThreeFull = 999999;
        public int secondsSinceLastFull = 999999;
        public int secondsSinceLastFiveMini = 999999;
        public int secondsSinceLastMini = 999999;

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned || p.surroundings == null)
                return ThoughtState.Inactive;

            return ThoughtState.ActiveAtStage(ThoughtStageIndex(p));
        }

        private int ThoughtStageIndex(Pawn p)
        {
            int miniTrees = 0;
            int fullTrees = 0;
            int superTrees = 0;

            List<TreeSighting> trees = new List<TreeSighting>(IdeoUtility.TreeSightingsNearPawn(p.Position, p.Map, p.Ideo));

            // Due to the actual tree counter being hardcoded for ideology for some reason, I had to make this eyesore
            if (!trees.NullOrEmpty())
                foreach (TreeSighting item in trees)
                {
                    if (item.Tree != null)
                    {
                        //_ = item.Tree.def.plant.treeCategory;
                        switch (item.Tree.def.plant.treeCategory)
                        {
                            case TreeCategory.Mini:
                                miniTrees++;
                                break;
                            case TreeCategory.Full:
                                fullTrees++;
                                break;
                            case TreeCategory.Super:
                                superTrees++;
                                break;
                        }
                    }

                }

            // Counters
            if (superTrees > 1)
            {
                secondsSinceLastMultiSuper = 0;
                secondsSinceLastSuper = 0;
            }
            else if (superTrees == 1)
            {
                secondsSinceLastMultiSuper++;
                secondsSinceLastSuper = 0;
            }
            else
            {
                secondsSinceLastMultiSuper++;
                secondsSinceLastSuper++;
            }

            if (fullTrees >= 5)
            {
                secondsSinceLastFiveFull = 0;
                secondsSinceLastThreeFull = 0;
                secondsSinceLastFull = 0;
            }
            else if (fullTrees >= 3)
            {
                secondsSinceLastFiveFull++;
                secondsSinceLastThreeFull = 0;
                secondsSinceLastFull = 0;
            }
            else if (fullTrees >= 1)
            {
                secondsSinceLastFiveFull++;
                secondsSinceLastThreeFull++;
                secondsSinceLastFull = 0;
            }
            else
            {
                secondsSinceLastFiveFull++;
                secondsSinceLastThreeFull++;
                secondsSinceLastFull++;
            }

            if (miniTrees >= 5)
            {
                secondsSinceLastFiveMini = 0;
                secondsSinceLastMini = 0;
            }
            else if (miniTrees >= 1)
            {
                secondsSinceLastFiveMini++;
                secondsSinceLastMini = 0;
            }
            else
            {
                secondsSinceLastFiveMini++;
                secondsSinceLastMini++;
            }

            // Sets variable to get lowest ticker
            int lowestTick = secondsSinceLastMini;
            if (secondsSinceLastFull < lowestTick) lowestTick = secondsSinceLastFull;
            if (secondsSinceLastSuper < lowestTick) lowestTick = secondsSinceLastSuper;

            if (secondsSinceLastMultiSuper < 42)
            {
                return 0;
            }
            if (secondsSinceLastSuper < 42)
            {
                return 1;
            }
            if (secondsSinceLastSuper <= 250 && secondsSinceLastThreeFull <= 250)
            {
                return 2;
            }
            if (secondsSinceLastSuper <= 250)
            {
                return 3;
            }
            if (secondsSinceLastFiveFull <= 250)
            {
                return 4;
            }
            if (secondsSinceLastThreeFull <= 250)
            {
                return 5;
            }
            if (secondsSinceLastFull <= 250 && secondsSinceLastMini <= 500)
            {
                return 6;
            }
            if (secondsSinceLastFull <= 250)
            {
                return 7;
            }
            if (secondsSinceLastFiveMini <= 500)
            {
                return 8;
            }
            if (secondsSinceLastMini <= 1000)
            {
                return 9;
            }
            if (lowestTick <= 2000)
            {
                return 10;
            }
            if (lowestTick <= 3000)
            {
                return 11;
            }
            if (lowestTick <= 4000)
            {
                return 12;
            }
            if (lowestTick <= 5000)
            {
                return 13;
            }
            return 14;
        }
    }
}

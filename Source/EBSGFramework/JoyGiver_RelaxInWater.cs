using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class JoyGiver_RelaxInWater : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.MapHeld == null || !ModsConfig.BiotechActive || pawn.genes == null) return null;
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension == null || extension.relatedGenes.NullOrEmpty()) return null;
            bool flag = true;
            foreach (GeneDef gene in extension.relatedGenes)
            {
                if (pawn.genes.HasGene(gene))
                {
                    flag = false;
                    break;
                }
            }
            if (flag) return null;
            if (EBSGUtilities.BadWeather(pawn.Map)) return null;

            if (!RCellFinder.TryFindRandomCellNearWith(pawn.Position, (IntVec3 p) => p.GetTerrain(pawn.Map).IsWater, pawn.Map, out IntVec3 result)) return null;
            return JobMaker.MakeJob(def.jobDef, result);
        }
    }
}

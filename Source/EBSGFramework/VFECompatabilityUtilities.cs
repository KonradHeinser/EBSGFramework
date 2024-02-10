using RimWorld;
using Verse;
using VanillaGenesExpanded;

namespace EBSGFramework
{
    public class VFECompatabilityUtilities
    {
        public static ThingDef BloodType(Pawn pawn)
        {
            if (StaticCollectionsClass.bloodtype_gene_pawns.ContainsKey(pawn))
            {
                return StaticCollectionsClass.bloodtype_gene_pawns[pawn];
            }
            return pawn.RaceProps.BloodDef;
        }
    }
}

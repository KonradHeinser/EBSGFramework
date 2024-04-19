using RimWorld;
using Verse;
using UnityEngine;

namespace EBSGFramework
{
    public class HediffComp_DRGOnKill : HediffComp
    {
        public HediffCompProperties_DRGOnKill Props => (HediffCompProperties_DRGOnKill)props;

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            if (Pawn.genes == null) return;

            if (!Props.resourceOffsets.NullOrEmpty())
                foreach (GeneLinker linker in Props.resourceOffsets)
                {
                    if (!EBSGUtilities.HasRelatedGene(Pawn, linker.mainResourceGene)) continue;

                    if (Pawn.genes.GetGene(linker.mainResourceGene) is ResourceGene resource)
                    {
                        if (parent.Severity < linker.minSeverity || parent.Severity > linker.maxSeverity ||
                            (linker.maxDistance > 0 && Pawn.Position.DistanceTo(victim.Position) > linker.maxDistance)) continue;

                        if ((victim.RaceProps.Humanlike && !linker.allowHumanoids) || (victim.RaceProps.Animal && !linker.allowAnimals) ||
                            (victim.RaceProps.Dryad && !linker.allowDryads) || (victim.RaceProps.Insect && !linker.allowInsects) ||
                            (victim.RaceProps.Insect && !linker.allowAnimals) || (victim.RaceProps.IsMechanoid && !linker.allowMechanoids) ||
                            (ModsConfig.AnomalyActive && victim.RaceProps.IsAnomalyEntity && !linker.allowEntities)) continue;

                        ResourceGene.OffsetResource(Pawn, linker.amount, resource, null, linker.usesGainStat);
                    }
                }
        }
    }
}

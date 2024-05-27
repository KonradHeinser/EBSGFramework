using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class Alert_LowResource : Alert
    {
        private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

        private List<string> targetLabels = new List<string>();

        private static EBSGCache_Component cache;

        public static EBSGCache_Component Cache
        {
            get
            {
                if (cache == null)
                    cache = Current.Game.GetComponent<EBSGCache_Component>();

                if (cache != null && cache.loaded)
                    return cache;
                return null;
            }
        }

        private List<GlobalTargetInfo> Targets
        {
            get
            {
                CalculateTargets();
                return targets;
            }
        }

        public override string GetLabel()
        {
            if (Targets.Count == 1)
            {
                return "AlertLowResource".Translate() + ": " + targetLabels[0];
            }
            return "AlertLowResource".Translate();
        }

        private void CalculateTargets()
        {
            targets.Clear();
            targetLabels.Clear();
            if (!ModsConfig.BiotechActive || (Cache != null && Cache.dynamicResourceGenes.NullOrEmpty()))
            {
                return;
            }
            foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
            {
                if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer && item.genes != null)
                {
                    ResourceGene resourceGene = item.genes?.GetFirstGeneOfType<ResourceGene>();
                    if (resourceGene == null) continue;
                    if (resourceGene.Value < resourceGene.MinLevelForAlert)
                    {
                        targets.Add(item);
                        targetLabels.Add(item.NameShortColored.Resolve());
                    }
                    else
                    {
                        if (Cache != null)
                        {
                            foreach (GeneDef gene in Cache.dynamicResourceGenes)
                                if (EBSGUtilities.HasRelatedGene(item, gene) && item.genes.GetGene(gene) is ResourceGene resource)
                                    if (resource.Value < resource.MinLevelForAlert)
                                    {
                                        targets.Add(item);
                                        targetLabels.Add(item.NameShortColored.Resolve());
                                        break;
                                    }
                        }
                        else
                            foreach (Gene gene in item.genes.GenesListForReading)
                            {
                                if (gene.def.HasModExtension<DRGExtension>() && gene.def.GetModExtension<DRGExtension>().isMainGene)
                                {
                                    resourceGene = (ResourceGene)gene;
                                    if (resourceGene.Value < resourceGene.MinLevelForAlert)
                                    {
                                        targets.Add(item);
                                        targetLabels.Add(item.NameShortColored.Resolve());
                                        break;
                                    }
                                }
                            }
                    }
                }
            }
        }

        public override TaggedString GetExplanation()
        {
            return "AlertLowResourceDesc".Translate() + ":\n" + targetLabels.ToLineList("  - ");
        }

        public override AlertReport GetReport()
        {
            if (Cache != null && Cache.dynamicResourceGenes.NullOrEmpty())
                return AlertReport.Inactive;
            return AlertReport.CulpritsAre(Targets);
        }
    }
}

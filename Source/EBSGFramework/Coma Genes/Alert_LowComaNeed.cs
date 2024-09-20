using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class Alert_LowComaNeed : Alert
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

        public Alert_LowComaNeed()
        {
            requireBiotech = true;
        }

        public override string GetLabel()
        {
            if (targets.Count == 1)
                return "EBSG_AlertLowComaPawn".Translate(targetLabels[0].Named("PAWN"));

            return "EBSG_AlertLowComaPawns".Translate(targetLabels.Count.ToStringCached().Named("NUMCULPRITS"));
        }

        private void CalculateTargets()
        {
            targets.Clear();
            targetLabels.Clear();
            foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
            {
                if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer)
                {
                    Need_ComaGene need_Coma = item.needs?.TryGetNeed<Need_ComaGene>();
                    if (need_Coma != null && need_Coma.CurLevel <= 0.1f && !item.Deathresting)
                    {
                        targets.Add(item);
                        targetLabels.Add(item.NameShortColored.Resolve());
                    }
                }
            }
        }

        public override TaggedString GetExplanation()
        {
            return "EBSG_LowComaNeedDesc".Translate(targetLabels.ToLineList("  - ").Named("CULPRITS"));
        }

        public override AlertReport GetReport()
        {
            if (Cache != null && !Cache.NeedComaAlert())
                return AlertReport.Inactive;

            CalculateTargets();
            return AlertReport.CulpritsAre(targets);
        }
    }
}

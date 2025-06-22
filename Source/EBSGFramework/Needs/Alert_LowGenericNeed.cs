using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Text;

namespace EBSGFramework
{
    public class Alert_LowGenericNeed : Alert
    {
        private List<Pawn> relatedColonistsResult = new List<Pawn>();

        private Dictionary<Pawn, string> needLabels = new Dictionary<Pawn, string>();

        private StringBuilder sb = new StringBuilder();

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

        private List<Pawn> RelatedColonists
        {
            get
            {
                relatedColonistsResult.Clear();
                needLabels.Clear();
                foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonistsAndPrisoners_NoCryptosleep)
                {
                    if (pawn.needs == null || pawn.needs.AllNeeds.NullOrEmpty()) continue;

                    foreach (Need need in pawn.needs.AllNeeds)
                    {
                        EBSGExtension extension = need.def.GetModExtension<EBSGExtension>();
                        if (extension != null && extension.displayLowAlert && !extension.thresholdPercentages.NullOrEmpty()
                                && need.CurLevel <= extension.thresholdPercentages[0])
                        {
                            relatedColonistsResult.Add(pawn);
                            needLabels.Add(pawn, need.LabelCap);
                            break;
                        }
                    }
                }
                return relatedColonistsResult;
            }
        }

        public Alert_LowGenericNeed()
        {
            defaultLabel = "EBSG_NeedLow".Translate();
            defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            sb.Length = 0;
            foreach (Pawn pawn in RelatedColonists)
                sb.AppendLine("  - " + pawn.NameShortColored.Resolve() + " : " + needLabels[pawn]);

            return "EBSG_NeedLowDescription".Translate(sb.ToString().TrimEndNewlines());
        }

        public override AlertReport GetReport()
        {
            if (cache != null && !cache.NeedNeedAlert())
                return AlertReport.Inactive;
            return AlertReport.CulpritsAre(RelatedColonists);
        }
    }
}

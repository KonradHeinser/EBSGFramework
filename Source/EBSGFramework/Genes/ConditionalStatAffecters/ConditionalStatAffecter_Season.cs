using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Season : ConditionalStatAffecter
    {
        public List<Season> seasons;

        public string label = null;

        public override string Label => GetLabel();

        public bool defaultActive;

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrLiteral();
            if (seasons.Count == 1)
                return "EBSG_SeasonOne".Translate(seasons.First().Label());
            return "EBSG_Season".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                Season currentSeason = GenLocalDate.Season(pawn);
                switch (currentSeason)
                {
                    case Season.Undefined:
                        return defaultActive;
                    case Season.PermanentSummer:
                        return seasons.Contains(Season.PermanentSummer) || seasons.Contains(Season.Summer);
                    case Season.PermanentWinter:
                        return seasons.Contains(Season.PermanentWinter) || seasons.Contains(Season.Winter);
                    default:
                        return seasons.Contains(currentSeason);
                }
            }
            return defaultActive;
        }
    }
}

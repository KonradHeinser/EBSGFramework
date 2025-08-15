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
            if (label != null) return label.TranslateOrFormat();
            if (seasons.Count == 1)
                return "EBSG_SeasonOne".Translate(seasons.First().Label());
            return "EBSG_Season".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
                return pawn.CheckSeason(seasons, defaultActive);
            return defaultActive;
        }
    }
}

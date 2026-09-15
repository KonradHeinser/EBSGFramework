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
            return seasons.Count == 1 ? "EBSG_SeasonOne".Translate(seasons.First().Label()) : "EBSG_Season".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
                return pawn.CheckSeason(seasons, defaultActive);
            return defaultActive;
        }
    }
}

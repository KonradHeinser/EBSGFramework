using Verse;

namespace EBSGFramework
{
    public class HediffComp_NameColor : HediffComp
    {
        public HediffCompProperties_NameColor Props => (HediffCompProperties_NameColor)props;

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

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Cache?.pawnNameColors.SetOrAdd(Pawn, Props.color);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Cache?.pawnNameColors?.Remove(Pawn);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                Cache?.pawnNameColors.SetOrAdd(Pawn, Props.color);
        }
    }
}

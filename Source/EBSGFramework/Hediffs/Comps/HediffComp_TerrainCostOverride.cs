using Verse;

namespace EBSGFramework
{
    public class HediffComp_TerrainCostOverride : HediffComp
    {
        public HediffCompProperties_TerrainCostOverride Props => (HediffCompProperties_TerrainCostOverride)props;

        private EBSGCache_Component cache;

        public EBSGCache_Component Cache
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
            if (Cache != null)
                cache.RegisterTerrainPawn(Pawn, parent);
        }

        public override void CompPostPostRemoved()
        {
            if (Cache != null)
                cache.DeRegisterTerrainPawn(Pawn);
        }
    }
}

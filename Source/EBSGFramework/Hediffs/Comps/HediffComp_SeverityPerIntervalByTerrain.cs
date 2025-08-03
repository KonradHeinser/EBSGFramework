using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityPerIntervalByTerrain : HediffComp
    {
        public HediffCompProperties_SeverityPerIntervalByTerrain Props => (HediffCompProperties_SeverityPerIntervalByTerrain)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(Props.interval))
            {
                if (Pawn.Spawned && ValidTerrain())
                    parent.Severity += Props.severityOnValid;
                else
                    parent.Severity += Props.severityOnInvalid;
            }
        }

        private bool ValidTerrain()
        {
            TerrainDef terrain = Pawn.Position.GetTerrain(Pawn.Map);

            switch (Props.pollution)
            {
                case TerrainPollution.Polluted:
                    if (!Pawn.Position.IsPolluted(Pawn.Map))
                        return false;
                    break;
                case TerrainPollution.Clean:
                    if (Pawn.Position.IsPolluted(Pawn.Map))
                        return false;
                    break;
                default:
                    break;
            }

            if (!Props.validTerrains.NullOrEmpty() && Props.validTerrains.Contains(terrain))
                return true;
            
            return EBSGUtilities.TagCheck(Props.validTerrainTags, terrain.tags);
        }
    }
}

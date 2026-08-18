using Verse;

namespace EBSGFramework
{
    public class HediffComp_FactionChangeOnDeath : HediffComp
    {
        private HediffCompProperties_FactionChangeOnDeath Props => (HediffCompProperties_FactionChangeOnDeath)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            
            var faction = Props.changeToKillerFaction && dinfo?.Instigator != null ? dinfo?.Instigator?.Faction : Props.factions.FindFirstFactionFromList();
            var recruiter = Props.changeToKillerFaction ? dinfo?.Instigator as Pawn : null;
            if (faction != Pawn.Faction)
                Pawn.SetFaction(faction, recruiter);
            
            if (Props.changeIdeoToPrimary && faction?.ideos?.PrimaryIdeo != null && Pawn.Ideo != faction.ideos.PrimaryIdeo)
                Pawn.ideo.SetIdeo(faction.ideos.PrimaryIdeo);
        }
    }
}
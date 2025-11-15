using Verse;

namespace EBSGFramework
{
    public class HediffComp_GiveHediffToKiller : HediffComp
    {
        private HediffCompProperties_GiveHediffToKiller Props => (HediffCompProperties_GiveHediffToKiller)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            if (!(dinfo?.Instigator is Pawn killer))
                return;
            
            if (Props.targetParams?.CanTarget(killer) == false)
                return;
            
            if (!Props.causes.NullOrEmpty())
                switch (Props.causeCheck)
                {
                    case CheckType.Required:
                        if (culprit == null || !Props.causes.Contains(culprit.def))
                            return;
                        break;
                    case CheckType.Forbidden:
                        if (culprit != null && Props.causes.Contains(culprit.def))
                            return;
                        break;
                    case CheckType.None:
                    default:
                        break;
                }
            
            if (Props.successChance?.Success(Pawn, killer) == false)
                return;
            
            killer.AddOrAppendHediff(Props.initialSeverity, Props.addedSeverity, Props.hediff, Pawn);
        }
    }
}
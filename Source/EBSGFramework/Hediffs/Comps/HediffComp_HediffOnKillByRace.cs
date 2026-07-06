using Verse;

namespace EBSGFramework
{
    public class HediffComp_HediffOnKillByRace : HediffComp
    {
        public HediffCompProperties_HediffOnKillByRace Props => (HediffCompProperties_HediffOnKillByRace)props;
        
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
            var severity  = Props.severity * (Props.multiplySeverityByBodySize ? victim.BodySize : 1f);
            var addedSeverity = Props.addedSeverity * (Props.multiplyAddedSeverityByBodySize ? victim.BodySize : 1f);
            if (victim.RaceProps.Humanlike)
            {
                if (Props.humanlike != null)
                    parent.pawn.AddOrAppendHediffs(severity, addedSeverity, Props.humanlike);
            }
            else if (victim.RaceProps.Dryad)
            {
                if (Props.dryad != null) 
                    parent.pawn.AddOrAppendHediffs(severity, addedSeverity, Props.dryad);
            }
            else if (victim.RaceProps.Insect)
            {
                if (Props.insect != null) 
                    parent.pawn.AddOrAppendHediffs(severity, addedSeverity, Props.insect);
            }
            else if (victim.RaceProps.Animal)
            {
                if (Props.animal != null) 
                    parent.pawn.AddOrAppendHediffs(severity, addedSeverity, Props.animal);
            }
            else if (victim.RaceProps.IsMechanoid)
            {
                if (Props.mechanoid != null) 
                    parent.pawn.AddOrAppendHediffs(severity, addedSeverity, Props.mechanoid);
            }
            else if (ModsConfig.AnomalyActive && victim.RaceProps.IsAnomalyEntity)
            {
                if (Props.entity != null) 
                    parent.pawn.AddOrAppendHediffs(severity, addedSeverity, Props.entity);
            }
            else
                parent.pawn.AddOrAppendHediffs(Props.severity, Props.addedSeverity, Props.defaultHediff);
        }
    }
}
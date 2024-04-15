using Verse;
namespace EBSGFramework
{
    public class HediffComp_HediffOnKill : HediffComp
    {
        private HediffCompProperties_HediffOnKill Props => (HediffCompProperties_HediffOnKill)props;

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            if (Props.hediff == null)
            {
                Log.Error(parent.def + " doesn't have a hediff set for the HediffOnKill comp.");
                return;
            }

            if (victim.RaceProps.Humanlike)
            {
                if (Props.allowHumanoids) AddHediffs(victim);
            }
            else if (victim.RaceProps.Dryad)
            {
                if (Props.allowDryads) AddHediffs(victim);
            }
            else if (victim.RaceProps.Insect)
            {
                if (Props.allowInsects) AddHediffs(victim);
            }
            else if (victim.RaceProps.Animal)
            {
                if (Props.allowAnimals) AddHediffs(victim);
            }
            else if (victim.RaceProps.IsMechanoid)
            {
                if (Props.allowMechanoids) AddHediffs(victim, false);
            }
            else if (ModsConfig.AnomalyActive && victim.RaceProps.IsAnomalyEntity)
            {
                if (Props.allowEntities) AddHediffs(victim);
            }
            else // As a default, just assume it's a viable target. This is for generic hopeful mod compatability
                EBSGUtilities.AddOrAppendHediffs(parent.pawn, Props.severityForFirstKill, Props.severityPerExtraKill, Props.hediff);
        }

        // Check gender is primarily for mechanoids, but may be used for other genderless things in the future
        private void AddHediffs(Pawn victim, bool checkGender = true)
        {
            if (checkGender && !CheckGenderViability(victim)) return;
            EBSGUtilities.AddOrAppendHediffs(parent.pawn, Props.severityForFirstKill, Props.severityPerExtraKill, Props.hediff);
        }

        private bool CheckGenderViability(Pawn victim)
        {
            if (!Props.onlyMaleVictims && !Props.onlyFemaleVictims) return true;
            if (Props.onlyMaleVictims && Props.onlyFemaleVictims) return false;
            Gender gender = victim.gender;
            if (gender == Gender.Male)
            {
                if (Props.onlyMaleVictims) return true;
            }
            else if (gender == Gender.Female)
            {
                if (Props.onlyFemaleVictims) return true;
            }
            return false;
        }
    }
}

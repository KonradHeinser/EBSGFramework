using Verse;

namespace EBSGFramework
{
    public class Need_Murderous : Need_GenericNeed
    {
        protected override FloatRange DefaultAgeRange => new FloatRange(13);

        protected override bool AgeCompatFlag => true;

        public Need_Murderous(Pawn newPawn) : base(newPawn)
        {
        }

        public void Notify_KilledPawn(DamageInfo? dinfo, Pawn victim)
        {
            /*
            if (Extension?.targetParams?.CanTarget(victim) == false)
                return;
            */
            if (Extension?.targetParams != null)
            { 
                if (!Extension.targetParams.CanTarget(victim))
                    return;
            }
            else if (Extension != null)
                if ((!Extension.allowHumanlikes && victim.RaceProps.Humanlike) || (!Extension.allowDryads && victim.RaceProps.Dryad) ||
                    (!Extension.allowInsects && victim.RaceProps.Insect) || (!Extension.allowAnimals && victim.RaceProps.Animal) ||
                    (!Extension.allowMechanoids && victim.RaceProps.IsMechanoid) ||
                    (ModsConfig.AnomalyActive && !Extension.allowEntities && victim.RaceProps.IsAnomalyEntity)) return;

            CurLevel += Extension?.increasePerKill ?? 1f;

            if (dinfo.HasValue && (dinfo?.WeaponBodyPartGroup != null || dinfo?.WeaponLinkedHediff != null || dinfo.Value.Weapon != null))
                if (dinfo?.WeaponBodyPartGroup != null || dinfo?.WeaponLinkedHediff != null || (dinfo.Value.Weapon != null && dinfo.Value.Weapon.IsMeleeWeapon))
                    CurLevel += Extension?.increasePerMeleeKill ?? 0f;
                else
                    CurLevel += Extension?.increasePerRangedKill ?? 0f;
        }
    }
}

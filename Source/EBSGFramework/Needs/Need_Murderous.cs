using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class Need_Murderous : Need
    {
        public bool alreadyChecked;

        public bool setThresholds;

        public float FallPerDay
        {
            get
            {
                if (Extension != null)
                    return Extension.fallPerDay;
                return 0.0333f;
            }
        }

        public float MinAgeForNeed
        {
            get
            {
                if (Extension != null)
                    return Extension.minAgeForNeed;
                return 13f;
            }
        }

        public float MaxAgeForNeed
        {
            get
            {
                if (Extension != null)
                    return Extension.maxAgeForNeed;
                return 9999f;
            }
        }

        public float IncreasePerKill
        {
            get
            {
                if (Extension != null)
                    return Extension.increasePerKill;
                return 1f;
            }
        }

        public float IncreasePerMeleeKill
        {
            get
            {
                if (Extension != null)
                    return Extension.increasePerMeleeKill;
                return 0f;
            }
        }

        public float IncreasePerRangedKill
        {
            get
            {
                if (Extension != null)
                    return Extension.increasePerRangedKill;
                return 0f;
            }
        }

        private EBSGExtension cachedExtension;

        public EBSGExtension Extension
        {
            get
            {
                if (cachedExtension == null && !alreadyChecked)
                {
                    cachedExtension = def.GetModExtension<EBSGExtension>();
                    alreadyChecked = true;
                }

                return cachedExtension;
            }
        }

        protected override bool IsFrozen
        {
            get
            {
                if (pawn.ageTracker.AgeBiologicalYearsFloat < MinAgeForNeed && pawn.ageTracker.AgeBiologicalYearsFloat > MaxAgeForNeed)
                    return true;

                return base.IsFrozen;
            }
        }

        public override bool ShowOnNeedList
        {
            get
            {
                if (pawn.ageTracker.AgeBiologicalYearsFloat < MinAgeForNeed && pawn.ageTracker.AgeBiologicalYearsFloat > MaxAgeForNeed)
                    return false;

                return base.ShowOnNeedList;
            }
        }

        public Need_Murderous(Pawn newPawn) : base(newPawn)
        {
            threshPercents = new List<float> { 0.3f };
        }

        public float FallMultiplier
        {
            get
            {
                if (Extension?.fallStat != null)
                    return pawn.StatOrOne(Extension.fallStat);
                return 1f;
            }
        }

        public override void NeedInterval()
        {
            if (!setThresholds && Extension != null)
            {
                threshPercents = Extension.thresholdPercentages;
                setThresholds = true;
            }

            if (!IsFrozen)
            {
                CurLevel -= FallPerDay / 400f * FallMultiplier;
                if (Extension != null && Extension.hediffWhenEmpty != null)
                {
                    if (CurLevel <= 0)
                        pawn.AddOrAppendHediffs(Extension.initialSeverity, Extension.risePerDayWhenEmpty / 400f, Extension.hediffWhenEmpty);
                    else
                        pawn.AddOrAppendHediffs(0, (Extension.fallPerDayWhenNotEmpty / 400f) * -1, Extension.hediffWhenEmpty);
                }
            }
        }

        public void Notify_KilledPawn(DamageInfo? dinfo, Pawn victim)
        {
            if (Extension != null)
                if ((!Extension.allowHumanlikes && victim.RaceProps.Humanlike) || (!Extension.allowDryads && victim.RaceProps.Dryad) ||
                    (!Extension.allowInsects && victim.RaceProps.Insect) || (!Extension.allowAnimals && victim.RaceProps.Animal) ||
                    (!Extension.allowMechanoids && victim.RaceProps.IsMechanoid) ||
                    (ModsConfig.AnomalyActive && !Extension.allowEntities && victim.RaceProps.IsAnomalyEntity)) return;

            CurLevel += IncreasePerKill;

            if (dinfo.HasValue && (dinfo?.WeaponBodyPartGroup != null || dinfo?.WeaponLinkedHediff != null || dinfo.Value.Weapon != null))
                if (dinfo?.WeaponBodyPartGroup != null || dinfo?.WeaponLinkedHediff != null || (dinfo.Value.Weapon != null && dinfo.Value.Weapon.IsMeleeWeapon))
                    CurLevel += IncreasePerMeleeKill;
                else
                    CurLevel += IncreasePerRangedKill;
        }
    }
}

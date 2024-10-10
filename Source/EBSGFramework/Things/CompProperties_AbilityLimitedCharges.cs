using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompProperties_AbilityLimitedCharges : CompProperties
    {
        public AbilityDef abilityDef;

        public int maxCharges = 1;

        public bool destroyAfterLast = true;

        public ThingDef spawnOnFinalUse;

        public int spawnCount = 1;

        public EffecterDef effecterOnFinalUse;

        public int effecterTicks = 0;

        public ThingDef filthOnFinalUse;

        public int filthCount = 1;

        [MustTranslate]
        public string chargeNoun = "charge";

        [MustTranslate]
        public string cooldownGerund = "on cooldown";

        public NamedArgument CooldownVerbArgument => cooldownGerund.CapitalizeFirst().Named("COOLDOWNGERUND");

        public NamedArgument ChargeNounArgument => chargeNoun.Named("CHARGENOUN");

        public CompProperties_AbilityLimitedCharges()
        {
            compClass = typeof(CompAbilityLimitedCharges);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }
            if (!req.HasThing)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_ReloadMaxCharges_Name".Translate(ChargeNounArgument), maxCharges.ToString(), "Stat_Thing_ReloadMaxCharges_Desc".Translate(ChargeNounArgument), 5440);
            }
        }
    }
}

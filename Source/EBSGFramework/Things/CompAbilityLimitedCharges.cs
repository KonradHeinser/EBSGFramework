using System.Collections.Generic;
using RimWorld.Utility;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityLimitedCharges : ThingComp, ICompWithCharges
    {
        CompProperties_AbilityLimitedCharges Props => (CompProperties_AbilityLimitedCharges)props;

        private Pawn Holder => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;

        private Ability ability;

        public Ability AbilityForReading
        {
            get
            {
                if (ability == null)
                {
                    ability = AbilityUtility.MakeAbility(Props.abilityDef, Holder);
                }
                return ability;
            }
        }

        public int MaxCharges => Props.maxCharges;

        public string LabelRemaining => $"{RemainingCharges} / {MaxCharges}";

        public int RemainingCharges
        {
            get
            {
                return AbilityForReading.RemainingCharges;
            }
            set
            {
                AbilityForReading.RemainingCharges = value;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (Holder != null)
            {
                AbilityForReading.pawn = Holder;
                AbilityForReading.verb.caster = Holder;
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            AbilityForReading.maxCharges = MaxCharges;
            RemainingCharges = MaxCharges;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            AbilityForReading.pawn = pawn;
            AbilityForReading.verb.caster = pawn;
            pawn.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            pawn.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public override string CompInspectStringExtra()
        {
            return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref ability, "ability");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Holder != null)
            {
                AbilityForReading.pawn = Holder;
                AbilityForReading.verb.caster = Holder;
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
            if (enumerable != null)
            {
                foreach (StatDrawEntry item in enumerable)
                {
                    yield return item;
                }
            }
            yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 5440);
        }

        public void UsedAbility(Ability ability)
        {
            if (ability != AbilityForReading) return;

            if (RemainingCharges == 0)
            {
                if (Holder.Map != null)// Map null check just in case the ability is usable on the world map
                {
                    if (Props.effecterOnFinalUse != null)
                    {
                        Effecter effecter = Props.effecterOnFinalUse.Spawn();
                        if (Props.effecterTicks != 0)
                        {
                            Holder.Map.effecterMaintainer.AddEffecterToMaintain(effecter, Holder.Position, Props.effecterTicks);
                        }
                        else
                        {
                            effecter.Trigger(new TargetInfo(Holder.Position, Holder.Map), new TargetInfo(Holder.Position, Holder.Map));
                            effecter.Cleanup();
                        }
                    }

                    if (Props.filthOnFinalUse != null)
                        FilthMaker.TryMakeFilth(Holder.Position, Holder.Map, Props.filthOnFinalUse, Props.filthCount);
                }

                if (Props.spawnOnFinalUse != null)
                {
                    Thing thing = ThingMaker.MakeThing(Props.spawnOnFinalUse, parent.Stuff);
                    thing.stackCount = Props.spawnCount;

                    Holder.inventory.TryAddAndUnforbid(thing);
                }
                if (Props.destroyAfterLast)
                    parent.Destroy();
            }
        }

        public bool CanBeUsed(out string reason)
        {
            reason = "";
            if (RemainingCharges <= 0)
            {
                reason = "CommandReload_NoCharges".Translate(Props.ChargeNounArgument);
                return false;
            }
            return true;
        }
    }
}

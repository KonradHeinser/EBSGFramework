using System.Collections.Generic;
using RimWorld;
using RimWorld.Utility;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityLimitedCharges : ThingComp, ICompWithCharges
    {
        CompProperties_AbilityLimitedCharges Props => (CompProperties_AbilityLimitedCharges)props;

        private Pawn Holder => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn ?? (parent?.ParentHolder as Pawn_ApparelTracker)?.pawn;

        private int remainingCharges;

        private int cooldownLeft;

        private Ability ability;

        public Ability AbilityForReading
        {
            get
            {
                if (ability == null)
                    MakeAbility();
                return ability;
            }
            set
            {
                ability = value;
            }
        }

        public void MakeAbility()
        {
            if (Holder == null) return;

            Ability oldAbility = Holder.abilities.GetAbility(Props.abilityDef);
            if (oldAbility != null && oldAbility.RemainingCharges == RemainingCharges)
            {
                ability = oldAbility;
                return;
            }

            ability = AbilityUtility.MakeAbility(Props.abilityDef, Holder);
            ability.maxCharges = Props.maxCharges;
            ability.RemainingCharges = RemainingCharges;
            ability.pawn = Holder;
            ability.verb.caster = Holder;
            if (cooldownLeft != 0 && Props.saveCooldown)
                ability.StartCooldown(cooldownLeft);
            Holder.abilities.abilities.Add(ability);
            Holder.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public int MaxCharges => Props.maxCharges;

        public string LabelRemaining => $"{RemainingCharges} / {MaxCharges}";

        public int RemainingCharges
        {
            get
            {
                return remainingCharges;
            }
            set
            {
                remainingCharges = value;
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
            RemainingCharges = MaxCharges;
            MakeAbility();
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            MakeAbility();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            cooldownLeft = pawn.abilities.GetAbility(Props.abilityDef)?.CooldownTicksRemaining ?? 0;
            pawn.abilities.RemoveAbility(Props.abilityDef);
            AbilityForReading = null;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (Holder != null)
                Holder.abilities.RemoveAbility(Props.abilityDef);
            AbilityForReading = null;
        }

        public override string CompInspectStringExtra()
        {
            return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingCharges, "remainingCharges", MaxCharges);
            Scribe_Values.Look(ref cooldownLeft, "cooldownLeft", 0);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
            if (enumerable != null)
                foreach (StatDrawEntry item in enumerable)
                    yield return item;

            yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 5440);
        }

        public void UsedAbility(Ability ability)
        {
            if (ability != AbilityForReading) return;
            remainingCharges--;

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
                    Thing thing = null;

                    if (Props.spawnOnFinalUse.MadeFromStuff)
                        thing = ThingMaker.MakeThing(Props.spawnOnFinalUse, Props.spawnStuffing ?? parent.Stuff);
                    else
                        thing = ThingMaker.MakeThing(Props.spawnOnFinalUse);

                    if (thing.TryGetQuality(out var newQuality) && parent.TryGetQuality(out var oldQuality))
                        thing.TryGetComp<CompQuality>().SetQuality(oldQuality, null); // No art stuff to avoid potential issues

                    thing.stackCount = Props.spawnCount;

                    if (Holder.Map != null) // This check avoids any errors/warnings about not being able to drop equipment. If they aren't on a map, it's just added to the inventory
                    {
                        if (thing is ThingWithComps compy && thing.def.equipmentType == EquipmentType.Primary && (Holder.equipment.Primary == null || (Holder.equipment.Primary == parent && Props.destroyAfterLast)))
                        {
                            Holder.equipment.AddEquipment(compy);
                            if (Props.destroyAfterLast)
                                parent.Destroy();
                            return;
                        }
                        if (thing is Apparel)
                        {
                            Apparel apparel = thing as Apparel;
                            if (ApparelUtility.HasPartsToWear(Holder, apparel.def) && apparel.PawnCanWear(Holder))
                            {
                                Apparel oldApparel = ApparelUtility.GetApparelReplacedByNewApparel(Holder, apparel);
                                if (oldApparel == null || (oldApparel == parent && Props.destroyAfterLast))
                                {
                                    Holder.apparel.Wear(apparel);
                                    if (Props.destroyAfterLast)
                                        parent.Destroy();
                                    return;
                                }
                            }
                        }
                    }

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

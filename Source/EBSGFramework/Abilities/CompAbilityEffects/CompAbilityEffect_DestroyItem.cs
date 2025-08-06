using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_DestroyItem : CompAbilityEffect
    {
        public new CompProperties_AbilityDestroyItem Props => props as CompProperties_AbilityDestroyItem;

        private List<ThingDef> options;

        public List<ThingDef> Options
        {
            get
            {
                if (options.NullOrEmpty())
                {
                    options = new List<ThingDef>();
                    foreach (var option in Props.options)
                        foreach (ThingLink link in option)
                            if (!options.Contains(link.thing))
                                options.Add(link.thing);
                }
                return options;
            }
        }

        private int minCount = -1;

        public int MinCount
        {
            get
            {
                if (minCount == -1)
                {
                    minCount = int.MaxValue;
                    foreach (var option in Props.options)
                        foreach (ThingLink link in option)
                        { 
                            if (link.amount < minCount)
                                minCount = link.amount;
                            if (minCount == 1)
                                break;
                        }
                }
                return minCount;
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (parent.def.EffectRadius <= 0)
            {
                if (!target.HasThing)
                    return false;

                Thing t = target.Thing;

                if (!Options.Contains(t.def)) // If none of the options use that def, no need to look further
                {
                    if (throwMessages)
                        Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_NoViableTarget".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                // This is the "looking further" referenced earlier
                foreach (var option in Props.options)
                {
                    bool flag = false;

                    foreach (ThingLink link in option)
                        if (link.thing == t.def && link.amount <= t.stackCount)
                        {
                            flag = true;
                            break;
                        }

                    if (!flag)
                    {
                        if (throwMessages)
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_NoViableTarget".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                        return false;
                    }
                }

                if (t.stackCount < MinCount) // If all of the options require more stuff than the stack has, no need to look furter
                {
                    if (throwMessages)
                        Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_InsufficientMaterial".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                    return false;
                }
            }
            else
            {
                if (!target.Cell.IsValid)
                    return false;
                var stuff = GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, parent.def.EffectRadius, true).ToList();

                if (stuff.NullOrEmpty())
                    return false;

                stuff = stuff.Where(arg => options.Contains(arg.def)).ToList();
                if (stuff.NullOrEmpty())
                {
                    if (throwMessages)
                        Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_NoViableTarget".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                stuff.SortBy(t => t.Position.DistanceTo(target.Cell));

                foreach (var option in Props.options)
                {
                    bool flag = false;

                    foreach (ThingLink link in option)
                    {
                        int amount = link.amount;

                        foreach (var thing in stuff)
                            if (thing.def == link.thing)
                                amount -= thing.stackCount;

                        if (amount <= 0)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        if (throwMessages)
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_InsufficientMaterial".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                        return false;
                    }
                }
            }

            return base.Valid(target, throwMessages);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (parent.def.EffectRadius <= 0)
            {
                if (target.Thing == null)
                    return; // Shouldn't ever happen, but better safe than erroring

                Thing t = target.Thing;

                foreach (var option in Props.options)
                    foreach (ThingLink link in option)
                        if (link.thing == t.def && link.amount <= t.stackCount)
                        {
                            if (t.stackCount == link.amount)
                                t.Destroy();
                            else
                                t.stackCount -= link.amount;
                            break;
                        }
            }
            else
            {
                var stuff = GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, parent.def.EffectRadius, true).Where(arg => options.Contains(arg.def)).ToList();

                if (stuff.NullOrEmpty())
                    return; // Shouldn't ever happen, but better safe than erroring

                foreach (var option in Props.options)
                    foreach (ThingLink link in option)
                    {
                        int amount = link.amount;
                        List<Thing> targets = new List<Thing>();
                        // Locate potential targets for the destruction
                        foreach (var t in stuff)
                            if (t.def == link.thing)
                            {
                                targets.Add(t);
                                amount -= t.stackCount;
                                if (amount <= 0)
                                    break;
                            }
                        
                        if (amount <= 0) // If enough items are found, then this option is valid
                        {
                            amount = link.amount; // Refresh the amount to use for deletion tally
                            foreach (Thing t in targets)
                                if (t.stackCount > amount)
                                {
                                    t.stackCount -= amount; // Final items for the option
                                    break;
                                }
                                else
                                {
                                    amount -= t.stackCount;
                                    t.Destroy();
                                    if (amount <= 0)
                                        break; // For the off chance that the stack count is the exact amount needed, or stack count is always 1
                                }
                            break; // Since this section is valid, none of the other options for the area need checked
                        }
                    }
            }
        }
    }
}

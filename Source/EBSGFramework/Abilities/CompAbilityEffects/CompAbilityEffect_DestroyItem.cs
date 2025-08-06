using System.Collections.Generic;
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
                    foreach (ThingLink link in Props.options)
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
                    foreach (ThingLink link in Props.options)
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
            if (!target.HasThing)
                return false;

            Thing t = target.Thing;

            if (t.stackCount < MinCount) // If all of the options require more stuff than the stack has, no need to look furter
            {
                if (throwMessages)
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_NoViableTarget".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (!Options.Contains(t.def)) // If none of the options use that def, no need to look further
            {
                if (throwMessages)
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_NoViableTarget".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
                return false;
            }
            
            // This is the "looking further" referenced earlier
            foreach (ThingLink link in Props.options)
                if (link.thing == t.def && link.amount <= t.stackCount)
                    return base.Valid(target, throwMessages);

            if (throwMessages)
                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_NoViableTarget".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target.Thing == null)
                return;

            Thing t = target.Thing;

            foreach (ThingLink link in Props.options)
                if (link.thing == t.def && link.amount <= t.stackCount)
                {
                    if (t.stackCount == link.amount)
                        t.Destroy();
                    else
                        t.stackCount -= link.amount;
                    break;
                }
        }
    }
}

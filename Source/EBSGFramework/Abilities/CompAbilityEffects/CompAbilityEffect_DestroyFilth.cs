using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class CompAbilityEffect_DestroyFilth : CompAbilityEffect
    {
        public new CompProperties_AbilityDestroyFilth Props => (CompProperties_AbilityDestroyFilth)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!target.Cell.IsValid)
                return false;

            var stuff = target.Cell.GetThingList(parent.pawn.Map);
            if (stuff.NullOrEmpty())
                return false;

            foreach (var thing in stuff) // Checks if there's a valid filth available
                if (thing is Filth filth && Props.amount.ValidValue(filth.thickness) && ValidThing(filth))
                    return base.Valid(target, throwMessages);

            if (throwMessages)
                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EBSG_NoViableTarget".Translate(), parent.pawn, MessageTypeDefOf.RejectInput, false);
            return false;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!target.Cell.IsValid)
                return;

            var stuff = target.Cell.GetThingList(parent.pawn.Map);
            
            if (!stuff.NullOrEmpty())
                foreach (var thing in stuff)
                    if (thing is Filth filth && Props.amount.ValidValue(filth.thickness) && ValidThing(filth))
                    {
                        if (Props.amount.max >= filth.thickness)
                            filth.Destroy();
                        else
                        {
                            filth.thickness -= Props.amount.max;
                            if (filth.Spawned)
                                filth.Map.mapDrawer.MapMeshDirty(filth.Position, MapMeshFlagDefOf.Things);
                        }
                        break;
                    }
        }

        private bool ValidThing(Thing thing)
        {
            ThingDef def = thing.def;
            if (!Props.validFilth.NullOrEmpty())
                return Props.validFilth.Contains(def);
            if (!Props.invalidFilth.NullOrEmpty())
                return !Props.invalidFilth.Contains(def);
            return true;
        }
    }
}

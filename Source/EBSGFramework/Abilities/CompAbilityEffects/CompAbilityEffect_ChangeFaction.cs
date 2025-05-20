using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace EBSGFramework
{
    public class CompAbilityEffect_ChangeFaction : CompAbilityEffect
    {
        public new CompProperties_AbilityChangeFaction Props => (CompProperties_AbilityChangeFaction)props;

        public Faction Faction
        {
            get
            {
                if (Props.useStatic)
                    return Find.FactionManager.FirstFactionOfDef(Props.staticFaction);
                return parent.pawn.Faction;
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages))
                return false;

            if (target.Pawn == null)
            {
                if (throwMessages)
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityTargetMustBePawn".Translate(), target.ToTargetInfo(Find.CurrentMap), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (target.Pawn.Faction == Faction)
            {
                if (throwMessages)
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityTargetNoFaction".Translate(Faction.Name), target.ToTargetInfo(Find.CurrentMap), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            // Final check is to catch any instance where valid was not checked first
            if (target.Pawn != null && target.Pawn?.Faction != Faction &&
                Props.successChance?.Success(parent.pawn, target.Pawn) != false)
            {
                Pawn t = target.Pawn;
                t.GetLord()?.RemovePawn(t);
                if (!Props.useStatic)
                {
                    Lord lord = parent.pawn.GetLord();
                    lord?.AddPawn(t);
                }
                t.SetFaction(Faction);
                // Make them one of the new faction's pawn kinds so they fit in better
                t.ChangeKind(Faction.RandomPawnKind());
            }
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (Props.successChance != null && target.Pawn != null)
                return "EBSG_SuccessChance".Translate(Math.Round(Props.successChance.Chance(parent.pawn, target.Thing == parent.pawn ? null : target.Thing) * 100, 3));
            return null;
        }
    }
}

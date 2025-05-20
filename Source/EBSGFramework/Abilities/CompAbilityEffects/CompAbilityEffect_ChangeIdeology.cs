using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_ChangeIdeology : CompAbilityEffect
    {
        public new CompProperties_AbilityChangeIdeology Props => (CompProperties_AbilityChangeIdeology)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!ModsConfig.IdeologyActive)
                return false;

            if (!base.Valid(target, throwMessages))
                return false;

            if (target.Pawn?.Ideo == null)
            {
                if (throwMessages)
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityTargetMustBePawn".Translate(), target.ToTargetInfo(Find.CurrentMap), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (ModsConfig.IdeologyActive && target.Pawn?.Ideo != null && 
                Props.successChance?.Success(parent.pawn, target.Pawn) != false)
            {
                // If a FactionDef is specified (for whatever reason), use the primary ideo of that faction
                Ideo ideo = Props.factionOfIdeo != null ? 
                    Find.FactionManager.FirstFactionOfDef(Props.factionOfIdeo)?.ideos?.PrimaryIdeo :
                    parent.pawn.Ideo; // Otherwise, we can use the caster's

                target.Pawn.ideo.SetIdeo(ideo);
                if (Props.certainty != FloatRange.Zero)
                    target.Pawn.ideo.OffsetCertainty(Props.certainty.RandomInRange - target.Pawn.ideo.Certainty);
            }
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (Props.successChance != null && ModsConfig.IdeologyActive && target.Pawn?.Ideo != null)
                return "EBSG_SuccessChance".Translate(Math.Round(Props.successChance.Chance(parent.pawn, target.Thing == parent.pawn ? null : target.Thing) * 100, 3));
            return null;
        }
    }
}

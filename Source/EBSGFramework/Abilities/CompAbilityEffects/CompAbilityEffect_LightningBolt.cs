using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_LightningBolt : CompAbilityEffect
    {
        public new CompProperties_AbilityLightingBolt Props => (CompProperties_AbilityLightingBolt)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            parent.pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(base.parent.pawn.Map, target.Cell));
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            string baseExplanation = "CannotUseAbility".Translate(parent.def.label) + ": ";

            Map map = parent.pawn.Map;

            if (map == null)
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + "AbilityCasterNoMap".Translate(), target.ToTargetInfo(map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (!target.Cell.InBounds(map)) return false;
            if (target.Cell.Roofed(map))
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + "Roofed".Translate(), target.ToTargetInfo(map), MessageTypeDefOf.RejectInput, false);

                return false;
            }
            return base.Valid(target, throwMessages);
        }
    }
}

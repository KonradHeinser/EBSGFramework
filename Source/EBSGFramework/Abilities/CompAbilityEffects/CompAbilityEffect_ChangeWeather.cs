using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_ChangeWeather : CompAbilityEffect
    {
        public new CompProperties_AbilityChangeWeather Props => (CompProperties_AbilityChangeWeather)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Map map;

            if (target != null && target.HasThing)
            {
                map = target.Thing.Map;
                if (map == null) map = target.Thing.MapHeld;
            }
            else
            {
                map = parent.pawn.Map;
                if (map == null) map = parent.pawn.MapHeld;
            }

            if (map != null) // Shouldn't happen, but want to avoid any weird fuckery
            {
                // Sets current weather to the desired one
                map.weatherManager.curWeather = Props.newWeather;
                map.weatherManager.TransitionTo(Props.newWeather);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            string baseExplanation = "CannotUseAbility".Translate(parent.def.label) + ": ";
            Map map;

            if (target != null && target.HasThing)
            {
                map = target.Thing.Map;
                if (map == null) map = target.Thing.MapHeld;
            }
            else
            {
                map = parent.pawn.Map;
                if (map == null) map = parent.pawn.MapHeld;
            }

            if (map == null)
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + "AbilityCasterNoMap".Translate(), target.ToTargetInfo(map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (Props.noCastIfAlreadyActive && map.weatherManager.curWeather == Props.newWeather)
            {
                if (throwMessages)
                    Messages.Message(baseExplanation + "AlreadyActiveWeather".Translate(Props.newWeather.LabelCap), target.ToTargetInfo(map), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            return true;
        }
    }
}

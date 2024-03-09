using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityChangeWeather : CompProperties_AbilityEffect
    {
        public WeatherDef newWeather;

        public bool noCastIfAlreadyActive = true;

        public CompProperties_AbilityChangeWeather()
        {
            compClass = typeof(CompAbilityEffect_ChangeWeather);
        }
    }
}

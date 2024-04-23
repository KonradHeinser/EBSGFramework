using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class StoryTellerComp_StorytellerAffinity : StorytellerComp
    {
        StoryTellerCompProperties_StorytellerAffinity Props => (StoryTellerCompProperties_StorytellerAffinity)props;

        private static EBSGCache_Component cache;

        public static EBSGCache_Component Cache
        {
            get
            {
                if (cache == null)
                    cache = Current.Game.GetComponent<EBSGCache_Component>();

                if (cache != null && cache.loaded)
                    return cache;
                return null;
            }
        }

        public int CurrentAffinity
        {
            get
            {
                if (Cache == null) return 0;
                return Cache.storyTellerAffinity; // Saved in cache because I don't know a way to save it in the teller
            }
        }

        public override void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
        {
            if (Cache == null) return;
            if (ev == AdaptationEvent.Died)
            {
                if (p.Faction != null && p.Faction.IsPlayer)
                {
                    if (p.RaceProps.Humanlike)
                    {
                        if (p.IsFreeColonist)
                            Cache.UpdateAffinity(Props.colonistDied, Props.adoredAffinity, Props.despisedAffinity);
                        else if (p.IsSlaveOfColony)
                            Cache.UpdateAffinity(Props.slaveDied, Props.adoredAffinity, Props.despisedAffinity);
                        else if (p.IsPrisonerOfColony)
                            Cache.UpdateAffinity(Props.prisonerDied, Props.adoredAffinity, Props.despisedAffinity);
                        else if (p.guest != null)
                            Cache.UpdateAffinity(Props.guestDied, Props.adoredAffinity, Props.despisedAffinity);
                        else // As a safety measure assume that it's a player pawn
                            Cache.UpdateAffinity(Props.colonistDied, Props.adoredAffinity, Props.despisedAffinity);
                    }
                }
                else
                {
                    if (!dinfo.HasValue) return;

                    DamageInfo damage = dinfo.Value;
                    if (damage.Instigator != null && damage.Instigator is Pawn killer && killer.IsColonist)
                        Cache.UpdateAffinity(Props.anyPawnKilled, Props.adoredAffinity, Props.despisedAffinity);
                }

                Log.Message(CurrentAffinity);
            }
            else if (ev == AdaptationEvent.Kidnapped)
            {
                if (p.IsColonist)
                    Cache.UpdateAffinity(Props.anyPawnKilled, Props.adoredAffinity, Props.despisedAffinity);
                Log.Message(CurrentAffinity);
            }
        }
    }
}

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class RitualDef : Def
    {
        public ResearchProjectDef researchPrerequisite;

        public bool godModeOnly = false;

        public List<IngredientCount> costs = new List<IngredientCount>();

        public List<IngredientCount> scalingCosts = new List<IngredientCount>();

        public List<RitualCompProperties> comps = new List<RitualCompProperties>();

        public bool sendReadyMessage = true;

        public string readyMessage;

        public bool allowInPocketMaps = false;

        public int cooldown = 60000;

        public List<RitualRole> roles = new List<RitualRole>();

        private static GameComponent_EBSGRitualManager manager;

        public static GameComponent_EBSGRitualManager Manager
        {
            get
            {
                if (manager == null)
                    manager = Current.Game.GetComponent<GameComponent_EBSGRitualManager>();

                return manager;
            }
        }

        public bool Visible
        {
            get
            {
                if (godModeOnly && !DebugSettings.godMode)
                    return false;
                if (researchPrerequisite?.IsFinished == false)
                    return false;
                return true;
            }
        }

        public AcceptanceReport Available(Map map, IntVec3 center, List<Pawn> participants)
        {
            var managerCheck = Manager.Available(this, map);
            if (managerCheck != true)
                return managerCheck;

            if (!comps.NullOrEmpty())
                foreach (var comp in comps)
                    if (!comp.Available(map, center, participants))
                        return false;

            using (new ProfilerBlock("Ritual supplies reachable"))
            {
                List<Thing> thingList = new List<Thing>();
                if (!costs.NullOrEmpty())
                    foreach (IngredientCount cost in costs)
                    {
                        List<Thing> matches;
                        using (new ProfilerBlock("ThingsMatchingFilter"))
                        {
                            matches = map.listerThings.ThingsMatchingFilter(cost.filter);
                        }
                        if (matches.NullOrEmpty())
                            return new AcceptanceReport("EBSG_RitualNoResource".Translate(cost.filter.Summary));
                        matches.SortBy((arg) => arg.PositionHeld.DistanceTo(center));

                        int required = Mathf.CeilToInt(cost.GetBaseCount());

                        foreach (Thing item in matches)
                        {
                            foreach (Pawn p in participants)
                                if (!item.Fogged() && !item.IsForbidden(p) &&
                                    p.CanReserveAndReach(item, PathEndMode.Touch, p.NormalMaxDanger()))
                                {
                                    required -= item.stackCount;
                                    thingList.Add(item);
                                    break;
                                }
                            if (required <= 0)
                                break;
                        }

                        if (required > 0)
                            return new AcceptanceReport("EBSG_RitualInsufficientResource".Translate(cost.filter.Summary));
                    }
                if (!scalingCosts.NullOrEmpty())
                    foreach (IngredientCount cost in scalingCosts)
                    {
                        List<Thing> matches;
                        using (new ProfilerBlock("ThingsMatchingFilter"))
                        {
                            matches = map.listerThings.ThingsMatchingFilter(cost.filter);
                        }
                        if (matches.NullOrEmpty())
                            return new AcceptanceReport("EBSG_RitualNoResource".Translate(cost.filter.Summary));
                        matches.SortBy((arg) => arg.PositionHeld.DistanceTo(center));

                        int required = Mathf.CeilToInt(cost.GetBaseCount()) * participants.Count;

                        foreach (Thing item in matches)
                        {
                            if (thingList.Contains(item)) continue;
                            foreach (Pawn p in participants)
                                if (!item.Fogged() && !item.IsForbidden(p) &&
                                    p.CanReserveAndReach(item, PathEndMode.Touch, p.NormalMaxDanger()))
                                {
                                    required -= item.stackCount;
                                    break;
                                }
                            if (required <= 0)
                                break;
                        }

                        if (required > 0)
                            new AcceptanceReport("EBSG_RitualInsufficientResource".Translate(cost.filter.Summary));
                    }
            }
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
                yield return item;

            if (roles.NullOrEmpty())
                yield return "has no roles, which means nobody can actually do the ritual.";

            if (comps.NullOrEmpty())
                yield return "has no comps, which will cause future errors.";
            else
                foreach (var comp in comps)
                    foreach (string item in comp.ConfigErrors())
                        yield return item;
        }
    }
}

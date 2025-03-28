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

        public int cooldown;

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

        public bool Available(Map map, IntVec3 center, List<Pawn> participants)
        {
            // Need to reference the component instead
            //if (cooldownTicks > 0)
            //  return false;
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
                            return false;
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
                            return false;
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
                            return false;
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
                    }
            }
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
                yield return item;

            if (comps.NullOrEmpty())
                yield return $"has no comps, which will cause future errors.";
            else
                foreach (var comp in comps)
                    foreach (string item in comp.ConfigErrors())
                        yield return item;
        }
    }
}

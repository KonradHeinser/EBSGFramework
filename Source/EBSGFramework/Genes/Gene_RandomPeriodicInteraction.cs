using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class Gene_RandomPeriodicInteraction : HediffAdder
    {
        public int nextInteract = 99;
        
        public override void PostAdd()
        {
            base.PostAdd();
            nextInteract = Extension.intervalRange.RandomInRange;
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            nextInteract -= delta;
            if (nextInteract <= 0 && pawn.Spawned && pawn.Faction != null && pawn.Awake()) // Make sure the pawn is on a map, in a faction, and not sleeping
            {
                nextInteract = Extension.intervalRange.RandomInRange; // Do this at the start to remove the risk that a pawn will start constantly trying to talk with people despite being alone
                var options = pawn.MapHeld.mapPawns.FreeHumanlikesSpawnedOfFaction(pawn.Faction).Where(p => p != pawn && p.Awake()).ToList(); // No gossiping about yourself. Sorry narcissists
                if (options.Any())
                {
                    var talkeeOptions = options.Where(p => p.Position.DistanceTo(pawn.Position) <= 5); // Find people to talk potentially talk to
                    if (!talkeeOptions.Any()) // If there are none, try again later
                        return;
                    var talkee = talkeeOptions.RandomElement();
                    options.Remove(talkee); // Ensures we won't talk about the person to their face
                    var target = Extension.thoughtAboutCarrier ? pawn : (options.Any() ? options.RandomElement() : null);
                    if (target != null)
                    {
                        if (Extension.interaction != null)
                            pawn.interactions.TryInteractWith(talkee, Extension.interaction);
                        if (Extension.interactionThought != null)
                            talkee.needs.mood.thoughts.memories.TryGainMemory(Extension.interactionThought, target);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextInteract, "nextInteract", Extension.intervalRange.RandomInRange);
        }
    }
}
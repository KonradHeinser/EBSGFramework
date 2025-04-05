using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse.AI;
using Verse.Noise;
using Verse;

namespace EBSGFramework
{
    public class Ritual : IExposable, ILoadReferenceable
    {
        public RitualDef def;

        public List<RitualComp> comps;

        public bool active = false;

        public int loadID = -1;

        public Ritual(RitualDef def)
        {
            this.def = def;
            if (!def.comps.NullOrEmpty())
            {
                comps = new List<RitualComp>();
                foreach (var comp in def.comps)
                {
                    try
                    {
                        RitualComp c = (RitualComp)Activator.CreateInstance(comp.compClass);
                        c.parent = this;
                        c.Initialize(comp);
                        comps.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{def.label} failed to instantiate or initialize the {comp.label} comp: " + ex);
                    }
                }
            }
            loadID = Find.UniqueIDsManager.GetNextPsychicRitualID(); // Using psychic ritual because it's close enough for my purposes

        }

        public void Tick()
        {
            // Most tick effects should have variations in each comp when they are active
            // Mote tick
            // Sound tick
            // Effecter Tick
            
            // Comps
        }

        public void Activate()
        {
            if (def.historyEvent != null)
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(def.historyEvent));
            Current.Game.GetComponent<GameComponent_EBSGRitualManager>().StartCooldown(def);
            if (!comps.NullOrEmpty())
                foreach (var comp in comps)
                {
                    if (comp.ActiveInRitual)
                        comp.DoEffects();
                }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            if (!comps.NullOrEmpty())
                foreach (var comp in comps)
                    comp.CompExposeData();
        }

        public string GetUniqueLoadID()
        {
            return "EBSGRitual_" + loadID;
        }
    }
}

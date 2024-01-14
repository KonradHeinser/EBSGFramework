using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class MultipleLives_Component : GameComponent
    {
        public bool loaded = false;
        private Dictionary<Pawn, Map> deadPawnMaps;
        private Dictionary<Pawn, HediffDef> deadPawnHediffs;
        private List<Pawn> deadPawns;

        public override void StartedNewGame()
        {
            StartUp();
        }

        public override void LoadedGame()
        {
            StartUp();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
        }

        private void AddPawnToLists(Pawn pawn, HediffDef hediffDef)
        {
            deadPawns.Add(pawn);
            deadPawnMaps.Add(pawn, pawn.Map);
            deadPawnHediffs.Add(pawn, hediffDef);
        }

        private void 

        private void StartUp()
        {
            //Map map = deadPawnMaps[deadPawns[0]];

            if (deadPawns == null)
            {
                deadPawns = new List<Pawn>();
            }

            if (deadPawnMaps == null)
            {
                deadPawnMaps = new Dictionary<Pawn, Map>();
            }

            if (deadPawnHediffs == null)
            {
                deadPawnHediffs = new Dictionary<Pawn, HediffDef>();
            }

            loaded = true;
        }
    }
}

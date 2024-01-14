using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EBSGFramework
{
    public class MultipleLives_Component : GameComponent
    {
        public bool loaded;
        public bool instantRevive;
        public int tick = 0;
        public Dictionary<Pawn, HediffDef> deadPawnHediffs;
        public Dictionary<Pawn, int> deadPawnTiles;
        public List<int> tiles;
        public List<HediffDef> hediffs;

        public List<Pawn> deadPawns;
        public List<Pawn> purgeList;

        public MultipleLives_Component(Game game)
        {
            loaded = false;
            deadPawns = new List<Pawn>();
            deadPawnHediffs = new Dictionary<Pawn, HediffDef>();
            deadPawnTiles = new Dictionary<Pawn, int>();
            purgeList = new List<Pawn>();
        }

        public override void StartedNewGame()
        {
            Initialize();
        }

        public override void LoadedGame()
        {
            Initialize();
        }

        public override void GameComponentTick()
        {
            tick++;
            tiles.Clear();
            hediffs.Clear();

            if (instantRevive && !deadPawns.NullOrEmpty())
            {
                instantRevive = false;
                foreach (Pawn pawn in deadPawns)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn]);
                    if (hediff != null)
                    {
                        HediffComp_MultipleLives multipleLivesComp = hediff.TryGetComp<HediffComp_MultipleLives>();
                        if (multipleLivesComp != null && multipleLivesComp.revivalProgress >= 1) ResurrectPawn(pawn);
                    }
                }
            }
            if (tick % 200 == 0 && !deadPawns.NullOrEmpty())
            {
                purgeList.Clear();
                tick = 0;
                foreach (Pawn pawn in deadPawns)
                {
                    if (!pawn.Dead || pawn.Corpse == null) purgeList.Add(pawn);
                    else
                    {
                        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn]);
                        if (hediff != null)
                        {
                            HediffComp_MultipleLives multipleLivesComp = hediff.TryGetComp<HediffComp_MultipleLives>();
                            if (multipleLivesComp != null)
                            {
                                float revivalSpeed = 0;
                                if (multipleLivesComp.Props.hoursToRevive > 0) revivalSpeed = 1 / multipleLivesComp.Props.hoursToRevive * 0.08f;
                                multipleLivesComp.revivalProgress += revivalSpeed;
                                if (multipleLivesComp.revivalProgress >= 1)
                                {
                                    ResurrectPawn(pawn);
                                }
                            }
                        }
                        else purgeList.Add(pawn);
                    }
                }
                if (!purgeList.NullOrEmpty())
                {
                    foreach (Pawn pawn in purgeList)
                    {
                        RemoveFromLists(pawn);
                    }
                }

                if (!deadPawns.NullOrEmpty())
                {
                    hediffs = deadPawnHediffs.Values.ToList();
                    tiles = deadPawnTiles.Values.ToList();
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref deadPawns, "EBSG_deadPawns", LookMode.Deep);
            if (!deadPawns.NullOrEmpty())
            {
                Scribe_Collections.Look(ref deadPawnHediffs, "EBSG_deadPawnHediffs", LookMode.Reference, LookMode.Def, ref deadPawns, ref hediffs);
                Scribe_Collections.Look(ref deadPawnTiles, "EBSG_deadPawnTiles", LookMode.Reference, LookMode.Value, ref deadPawns, ref tiles);
            }
        }

        public void AddPawnToLists(Pawn pawn, HediffDef hediffDef, bool startRevive = false)
        {
            if (!pawn.Dead) return; // Not sure how this would happen, but I ain't messin with Murphy that often
            deadPawns.Add(pawn);
            deadPawnHediffs.Add(pawn, hediffDef);
            deadPawnTiles.Add(pawn, pawn.Corpse.Map.Tile);
            instantRevive = startRevive;
        }

        public void ResurrectPawn(Pawn pawn)
        {
            if (pawn.Dead && pawn.Corpse != null)
            {
                if (pawn.Corpse.MapHeld != null && pawn.Corpse.Spawned)
                {
                    EBSGUtilities.TryToRevivePawn(pawn);
                }
                else
                {
                    if (pawn.Corpse.MapHeld != null)
                    {
                        Thing storage = pawn.Corpse.StoringThing(); // If not spawned but it has a map, then it may just be in a container
                        if (storage != null)
                        {
                            GenSpawn.Spawn(pawn.Corpse, storage.Position, storage.Map);
                            EBSGUtilities.TryToRevivePawn(pawn);
                        }
                    }
                    else
                    {
                        int tile = -1;
                        Caravan caravan = null;
                        Thing storage = pawn.Corpse.StoringThing(); // Be sure there's no weird container business involved
                        if (storage != null)
                        {
                            GenSpawn.Spawn(pawn.Corpse, storage.Position, storage.Map);
                        }

                        tile = pawn.Corpse.Tile;

                        // See if a caravan is holding onto the corpse. Starts by checking tile because it's probably faster than checking each container
                        List<Caravan> caravans = Find.World.worldObjects.Caravans.Where((Caravan c) => c.Tile == pawn.Corpse.Tile && c.AllThings.Contains(pawn.Corpse)).ToList();
                        if (!caravans.NullOrEmpty())
                        {
                            caravan = caravans[0];
                        }
                        else
                        {
                            // If no caravan was found that way, see if there happens to be any caravan on the tile in the pawn's faction
                            caravans = Find.World.worldObjects.Caravans.Where((Caravan c) => c.Tile == pawn.Corpse.Tile && c.Faction == pawn.Faction).ToList();
                            if (!caravans.NullOrEmpty())
                            {
                                caravan = caravans[0];
                            }
                        }
                        EBSGUtilities.TryToRevivePawn(pawn);
                        if (caravan != null) caravan.AddPawn(pawn, false);
                        else if (pawn.Faction.IsPlayer)
                        {
                            List<Pawn> pawns = new List<Pawn>();
                            pawns.Add(pawn);
                            CaravanMaker.MakeCaravan(pawns, pawn.Faction, deadPawnTiles[pawn], false); // Creates caravan on death tile if all else fails
                        }
                    }
                }
            }
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn]);
            if (hediff != null)
            {
                HediffComp_MultipleLives multipleLivesComp = hediff.TryGetComp<HediffComp_MultipleLives>();
                if (multipleLivesComp != null)
                {
                    multipleLivesComp.revivalProgress = 0;
                    multipleLivesComp.pawnReviving = false;
                }
            }
            purgeList.Add(pawn);
        }

        private void RemoveFromLists(Pawn pawn)
        {
            deadPawns.Remove(pawn);
            deadPawnHediffs.Remove(pawn);
            deadPawnTiles.Remove(pawn);
        }

        private void Initialize()
        {
            tick = 0;
            loaded = true;

            // The expose data's should prevent this, but Murphy Murphy yada yada
            if (deadPawns == null)
            {
                deadPawns = new List<Pawn>();
            }

            if (deadPawnHediffs == null)
            {
                deadPawnHediffs = new Dictionary<Pawn, HediffDef>();
            }

            if (deadPawnTiles == null)
            {
                deadPawnTiles = new Dictionary<Pawn, int>();
            }

            if (purgeList == null)
            {
                purgeList = new List<Pawn>();
            }

            if (tiles == null)
            {
                tiles = new List<int>();
            }

            if (hediffs == null)
            {
                hediffs = new List<HediffDef>();
            }
        }
    }
}

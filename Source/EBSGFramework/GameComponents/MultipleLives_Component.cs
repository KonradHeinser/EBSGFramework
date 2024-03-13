using System;
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
        public bool newDead;
        public bool instantRevive;
        public int tick = 0;
        public static Dictionary<Pawn, HediffDef> deadPawnHediffs;

        public List<Corpse> deadPawns;
        public List<Corpse> purgeList;
        public List<string> indestructibleLabels;

        public MultipleLives_Component(Game game)
        {
            loaded = false;
            deadPawns = new List<Corpse>();
            deadPawnHediffs = new Dictionary<Pawn, HediffDef>();
            purgeList = new List<Corpse>();
            indestructibleLabels = new List<string>();
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

            if (newDead)
            {
                newDead = false;
                foreach (Corpse pawn in deadPawns)
                {
                    bool flag = true;
                    Hediff hediff = pawn.InnerPawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn.InnerPawn]);
                    if (hediff != null)
                    {
                        HediffComp_MultipleLives multipleLivesComp = hediff.TryGetComp<HediffComp_MultipleLives>();
                        if (multipleLivesComp != null && multipleLivesComp.Props.indestructibleWhileResurrecting)
                        {
                            if (!pawn.AllComps.NullOrEmpty())
                            {
                                foreach (ThingComp comp in pawn.AllComps)
                                {
                                    if (comp.GetType() == typeof(CompIndestructible))
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else flag = false;
                    }

                    if (flag)
                    {
                        CompIndestructible indestructibleComp = (CompIndestructible)Activator.CreateInstance(typeof(CompIndestructible));
                        indestructibleComp.parent = pawn;
                        pawn.AllComps.Add(indestructibleComp);
                        indestructibleComp.Initialize(new CompProperties_Indestructible());
                    }
                }
            }

            if (instantRevive && !deadPawns.NullOrEmpty())
            {
                instantRevive = false;
                foreach (Corpse pawn in deadPawns)
                {
                    Hediff hediff = pawn.InnerPawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn.InnerPawn]);
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
                foreach (Corpse pawn in deadPawns)
                {
                    if (pawn == null || pawn.DestroyedOrNull() || !RecordPawnData(pawn.InnerPawn)) purgeList.Add(pawn);
                    else
                    {
                        Hediff hediff = pawn.InnerPawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn.InnerPawn]);
                        if (hediff != null)
                        {
                            HediffComp_MultipleLives multipleLivesComp = hediff.TryGetComp<HediffComp_MultipleLives>();
                            if (multipleLivesComp != null)
                            {
                                float revivalSpeed = 0;
                                if (multipleLivesComp.hoursToRevive > 0) revivalSpeed = 1 / multipleLivesComp.hoursToRevive * 0.08f;
                                multipleLivesComp.revivalProgress += revivalSpeed;
                                if (multipleLivesComp.revivalProgress >= 1)
                                {
                                    ResurrectPawn(pawn);
                                    purgeList.Add(pawn);
                                }
                            }
                        }
                        else purgeList.Add(pawn);
                    }
                }
                if (!purgeList.NullOrEmpty())
                {
                    foreach (Corpse pawn in purgeList)
                    {
                        deadPawns.Remove(pawn);
                    }
                }
            }
        }

        public static bool RecordPawnData(Pawn pawn)
        {
            if (pawn.health == null || pawn.health.hediffSet.hediffs.NullOrEmpty()) return false;

            if (deadPawnHediffs.ContainsKey(pawn))
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn]);
                if (hediff == null)
                {
                    deadPawnHediffs.Remove(pawn);
                }
                else
                {
                    HediffComp_MultipleLives comp = hediff.TryGetComp<HediffComp_MultipleLives>();
                    if (comp != null && comp.pawnReviving)
                    {
                        return true;
                    }

                    deadPawnHediffs.Remove(pawn);
                }
            }

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                HediffComp_MultipleLives comp = hediff.TryGetComp<HediffComp_MultipleLives>();
                if (comp != null && comp.pawnReviving)
                {
                    deadPawnHediffs.Add(pawn, hediff.def);
                    return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref deadPawns, "EBSG_deadPawns", LookMode.Reference);
        }

        public void AddPawnToLists(Pawn pawn, HediffDef hediffDef, bool startRevive = false)
        {
            if (!pawn.Dead) return; // Not sure how this would happen, but I ain't messin with Murphy that often
            deadPawns.Add(pawn.Corpse);
            deadPawnHediffs.Add(pawn, hediffDef);
            instantRevive = startRevive;
            newDead = true;
        }

        public void ResurrectPawn(Corpse pawn)
        {
            indestructibleLabels = new List<string>();
            Hediff hediff = pawn.InnerPawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn.InnerPawn]);
            if (hediff != null)
            {
                HediffComp_MultipleLives multipleLivesComp = hediff.TryGetComp<HediffComp_MultipleLives>();
                if (multipleLivesComp != null)
                {
                    multipleLivesComp.revivalProgress = 0;
                    multipleLivesComp.pawnReviving = false;
                }
            }

            if (pawn != null && pawn.InnerPawn.Dead)
            {
                if (pawn.MapHeld != null && pawn.Spawned)
                {
                    EBSGUtilities.TryToRevivePawn(pawn.InnerPawn);
                }
                else
                {
                    if (pawn.MapHeld != null)
                    {
                        Thing storage = pawn.StoringThing(); // If not spawned but it has a map, then it may just be in a container
                        if (storage != null)
                        {
                            GenSpawn.Spawn(pawn, storage.Position, storage.Map);
                            EBSGUtilities.TryToRevivePawn(pawn.InnerPawn);
                        }
                    }
                    else
                    {
                        int tile = -1;
                        Caravan caravan = null;
                        Thing storage = pawn.StoringThing(); // Be sure there's no weird container business involved
                        if (storage != null)
                        {
                            GenSpawn.Spawn(pawn, storage.Position, storage.Map);
                        }

                        tile = pawn.Tile;

                        // See if a caravan is holding onto the corpse. Starts by checking tile because it's probably faster than checking each container
                        List<Caravan> caravans = Find.World.worldObjects.Caravans.Where((Caravan c) => c.Tile == pawn.Tile && c.AllThings.Contains(pawn)).ToList();
                        if (!caravans.NullOrEmpty())
                        {
                            caravan = caravans[0];
                        }
                        else
                        {
                            // If no caravan was found that way, see if there happens to be any caravan on the tile in the pawn's faction
                            caravans = Find.World.worldObjects.Caravans.Where((Caravan c) => c.Tile == pawn.Tile && c.Faction == pawn.Faction).ToList();
                            if (!caravans.NullOrEmpty())
                            {
                                caravan = caravans[0];
                            }
                        }
                        EBSGUtilities.TryToRevivePawn(pawn.InnerPawn);
                        if (caravan != null) caravan.AddPawn(pawn.InnerPawn, false);
                        else if (pawn.Faction.IsPlayer)
                        {
                            List<Pawn> pawns = new List<Pawn>();
                            pawns.Add(pawn.InnerPawn);
                            Hediff revivalHediff = pawn.InnerPawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn.InnerPawn]);
                            if (revivalHediff != null)
                            {
                                HediffComp_MultipleLives comp = revivalHediff.TryGetComp<HediffComp_MultipleLives>();
                                if (comp != null)
                                {
                                    CaravanMaker.MakeCaravan(pawns, pawn.Faction, comp.deathTile, false); // Creates caravan on death tile if all else fails
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Initialize()
        {
            tick = 0;
            loaded = true;

            // The expose data's should prevent this, but Murphy Murphy yada yada
            if (deadPawns == null)
            {
                deadPawns = new List<Corpse>();
            }

            if (deadPawnHediffs == null)
            {
                deadPawnHediffs = new Dictionary<Pawn, HediffDef>();
            }

            if (purgeList == null)
            {
                purgeList = new List<Corpse>();
            }

            indestructibleLabels = new List<string>();
        }
    }
}

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
        public int tick;
        public static Dictionary<Pawn, HediffDef> deadPawnHediffs;

        public List<Corpse> deadPawns;
        public List<Corpse> forbiddenCorpses;
        public List<Corpse> purgeList;

        public MultipleLives_Component(Game game)
        {
            loaded = false;
            deadPawns = new List<Corpse>();
            deadPawnHediffs = new Dictionary<Pawn, HediffDef>();
            purgeList = new List<Corpse>();
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

            if (newDead && !deadPawns.NullOrEmpty())
            {
                newDead = false;
                foreach (Corpse pawn in deadPawns)
                {
                    if (pawn == null || pawn.DestroyedOrNull() || !RecordPawnData(pawn.InnerPawn) || !pawn.PawnHasAnyHediff()) continue;

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
                    if (pawn == null || pawn.DestroyedOrNull() || !RecordPawnData(pawn.InnerPawn) || !pawn.PawnHasAnyHediff()) continue;

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
                    if (pawn == null || pawn.DestroyedOrNull() || !RecordPawnData(pawn.InnerPawn) || !pawn.PawnHasAnyHediff()) purgeList.Add(pawn);
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
                                if (!pawn.IsForbidden(Faction.OfPlayer) && multipleLivesComp.Props.indestructibleWhileResurrecting && multipleLivesComp.Props.alwaysForbiddenWhileResurrecting && (pawn.Faction == null || !pawn.Faction.IsPlayer))
                                    pawn.SetForbidden(true, false);
                            }
                        }
                        else purgeList.Add(pawn);
                    }
                }
                if (!purgeList.NullOrEmpty())
                {
                    if (purgeList.Count == deadPawns.Count)
                    {
                        deadPawns.Clear();
                        deadPawnHediffs.Clear();
                        forbiddenCorpses.Clear();
                    }
                    else
                        foreach (Corpse pawn in purgeList)
                        {
                            RemovePawnFromLists(pawn);
                        }
                }
            }
        }

        public static bool RecordPawnData(Corpse corpse)
        {
            if (corpse == null || corpse.InnerPawn == null) return false;
            return RecordPawnData(corpse.InnerPawn);
        }

        public static bool RecordPawnData(Pawn pawn)
        {
            if (!pawn.PawnHasAnyHediff()) return false;
            if (deadPawnHediffs.ContainsKey(pawn))
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn]);
                if (hediff == null)
                    deadPawnHediffs.Remove(pawn);
                else
                {
                    HediffComp_MultipleLives comp = hediff.TryGetComp<HediffComp_MultipleLives>();
                    if (comp != null && comp.pawnReviving)
                        return true;

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

        public void AddPawnToLists(Pawn pawn, HediffDef hediffDef, bool startRevive = false, bool forbidden = false)
        {
            if (!pawn.Dead || pawn.Corpse == null) return; // Not sure how this would happen, but I ain't messin with Murphy that often
            if (!deadPawns.Contains(pawn.Corpse))
                deadPawns.Add(pawn.Corpse);
            deadPawnHediffs.Remove(pawn);

            if (forbidden && !forbiddenCorpses.Contains(pawn.Corpse))
                forbiddenCorpses.Add(pawn.Corpse);

            deadPawnHediffs.Add(pawn, hediffDef);
            instantRevive = startRevive;
            newDead = true;
        }

        public void RemovePawnFromLists(Corpse pawn)
        {
            if (pawn != null)
            {
                if (deadPawns.Contains(pawn))
                    deadPawns.Remove(pawn);
                if (pawn.InnerPawn != null)
                    deadPawnHediffs.Remove(pawn.InnerPawn);
                if (forbiddenCorpses.Contains(pawn))
                    forbiddenCorpses.Remove(pawn);
            }
        }

        public static void ResurrectPawn(Corpse pawn)
        {
            if (pawn?.InnerPawn == null || !RecordPawnData(pawn.InnerPawn)) return;
            Hediff hediff = pawn.InnerPawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn.InnerPawn]);

            bool removeAllInjuries = false;

            HediffComp_MultipleLives multipleLivesComp = null;

            if (hediff != null)
            {
                multipleLivesComp = hediff.TryGetComp<HediffComp_MultipleLives>();
                if (multipleLivesComp != null)
                {
                    multipleLivesComp.revivalProgress = 0;
                    multipleLivesComp.pawnReviving = false;
                    removeAllInjuries = multipleLivesComp.Props.removeAllInjuriesAfterRevival;
                }
            }

            // This is separate because I may be adding more ways to set up injury removal in the future (i.e. remove only in certain conditions)
            if (removeAllInjuries)
            {
                List<Hediff> hediffs = new List<Hediff>(pawn.InnerPawn.health.hediffSet.hediffs.Where(h => h.def.injuryProps != null));
                pawn.InnerPawn.RemoveAllOfHediffs(hediffs);
            }

            if (pawn.InnerPawn.Dead && multipleLivesComp != null)
            {
                if (pawn.InnerPawn.Faction.IsPlayer)
                {
                    if (multipleLivesComp.Props.revivalSuccessMessage != null)
                        Messages.Message(multipleLivesComp.Props.revivalSuccessMessage.TranslateOrFormat(pawn.InnerPawn.LabelShortCap, multipleLivesComp.livesLeft.ToString()),
                            MessageTypeDefOf.PositiveEvent);
                    if (multipleLivesComp.Props.revivalSuccessLetterLabel != null)
                    {
                        Letter letter = LetterMaker.MakeLetter(multipleLivesComp.Props.revivalSuccessLetterLabel.TranslateOrFormat(pawn.InnerPawn.LabelShortCap, multipleLivesComp.livesLeft.ToString()),
                            multipleLivesComp.Props.revivalSuccessLetterDescription.TranslateOrFormat(pawn.InnerPawn.LabelShortCap, multipleLivesComp.livesLeft.ToString()),
                            LetterDefOf.PositiveEvent);
                        Find.LetterStack.ReceiveLetter(letter);
                    }
                }

                if (pawn.MapHeld != null && pawn.Spawned)
                {
                    pawn.InnerPawn.TryToRevivePawn();
                }
                else
                {
                    if (pawn.MapHeld != null)
                    {
                        Thing storage = pawn.StoringThing(); // If not spawned but it has a map, then it may just be in a container
                        if (storage != null)
                        {
                            var position = storage.Position;
                            var map = storage.Map;
                            GenSpawn.Spawn(pawn, position, map);
                            pawn.InnerPawn.TryToRevivePawn();
                            EBSGUtilities.ThingAndSoundMaker(position, map, multipleLivesComp.Props.thingSpawnOnReviveEnd, multipleLivesComp.Props.thingsToSpawnOnReviveEnd,
                                multipleLivesComp.Props.reviveEndSound);
                        }
                    }
                    else
                    {
                        Caravan caravan = null;

                        // See if a caravan is holding onto the corpse. Starts by checking tile because it's probably faster than checking each container
                        var caravans = Find.World.worldObjects.Caravans.Where(c => c.Tile == pawn.Tile && c.AllThings.Contains(pawn));
                        if (!caravans.EnumerableNullOrEmpty())
                            caravan = caravans.First();
                        else
                        {
                            // If no caravan was found that way, see if there happens to be any caravan on the tile in the pawn's faction
                            caravans = Find.World.worldObjects.Caravans.Where(c => c.Tile == pawn.Tile && c.Faction == pawn.Faction);
                            if (!caravans.EnumerableNullOrEmpty())
                                caravan = caravans.First();
                        }
                        pawn.InnerPawn.TryToRevivePawn();
                        if (caravan != null) caravan.AddPawn(pawn.InnerPawn, false);
                        else if (pawn.Faction.IsPlayer)
                        {
                            List<Pawn> pawns = new List<Pawn> { pawn.InnerPawn };
                            CaravanMaker.MakeCaravan(pawns, pawn.Faction, multipleLivesComp.deathTile, false); // Creates caravan on death tile if all else fails
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
                deadPawns = new List<Corpse>();

            if (deadPawnHediffs == null)
                deadPawnHediffs = new Dictionary<Pawn, HediffDef>();

            if (purgeList == null)
                purgeList = new List<Corpse>();

            if (forbiddenCorpses == null)
            {
                forbiddenCorpses = new List<Corpse>();
                if (!deadPawns.NullOrEmpty())
                    foreach (Corpse pawn in deadPawns)
                        if (RecordPawnData(pawn))
                        {
                            Hediff hediff = pawn.InnerPawn.health.hediffSet.GetFirstHediffOfDef(deadPawnHediffs[pawn.InnerPawn]);

                            HediffComp_MultipleLives multipleLivesComp = hediff?.TryGetComp<HediffComp_MultipleLives>();
                            if (multipleLivesComp != null)
                                forbiddenCorpses.Add(pawn);
                        }
            }
        }
    }
}

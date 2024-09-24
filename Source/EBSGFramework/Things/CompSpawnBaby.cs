using Verse;
using RimWorld;
using System.Collections.Generic;
using RimWorld.Planet;

namespace EBSGFramework
{
    public class CompSpawnBaby : ThingComp
    {
        public CompProperties_SpawnBaby Props => (CompProperties_SpawnBaby)props;

        public int spawnLeft;

        public int ticksLeft;

        public Pawn mother;

        public Pawn father;

        public List<GeneDef> genes;

        public List<GeneDef> Genes
        {
            get
            {
                if (genes.NullOrEmpty())
                {
                    genes = new List<GeneDef>();
                    if (Props.staticXenotype != null)
                        genes = Props.staticXenotype.genes;
                    else if (Props.xenotypeSource == XenoSource.Father && father != null)
                        genes = PregnancyUtility.GetInheritedGenes(father, null);
                    else if (Props.xenotypeSource == XenoSource.Mother && mother != null)
                        genes = PregnancyUtility.GetInheritedGenes(null, mother);
                    else
                        genes = PregnancyUtility.GetInheritedGenes(father, mother);
                }

                return genes;
            }
        }

        public Faction faction;

        public override void PostPostMake()
        {
            base.PostPostMake();
            spawnLeft = Props.maxTotalSpawn;
            ticksLeft = Props.completionTicks.RandomInRange;

            if (faction == null)
                if (mother != null || father != null) faction = mother?.Faction ?? father.Faction;
                else if (parent.Faction != null) faction = parent.Faction;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (spawnLeft > 0 || spawnLeft == -1)
            {
                if (parent.HasComp<CompRefuelable>())
                {
                    CompRefuelable fuel = parent.GetComp<CompRefuelable>();
                    if (!fuel.HasFuel) return;
                }

                // Ensures plants and pawns are grown before they start creating new pawns
                bool flag = false;
                if (parent is Plant plant && plant.Growth < 1f)
                    flag |= plant.Growth < 1f;
                else if (parent is Pawn carrier)
                    if (!carrier.RaceProps.IsMechanoid)
                        flag |= !carrier.ageTracker.Adult;

                if (flag) return;

                ticksLeft--;

                if (ticksLeft <= 0)
                {
                    Map map = parent.MapHeld;
                    Caravan caravan = null;
                    if (map == null && parent is Pawn carrier) caravan = carrier.GetCaravan();
                    if (map == null && caravan == null) return;

                    int numberToSpawn = Props.spawnPerCompletion.RandomInRange;
                    List<IntVec3> alreadyUsedSpots = new List<IntVec3>();

                    if (spawnLeft != -1)
                    {
                        if (numberToSpawn > spawnLeft)
                            numberToSpawn = spawnLeft;
                        spawnLeft -= numberToSpawn;
                    }

                    for (int i = 0; i < numberToSpawn; i++)
                    {
                        // If the faction is somehow null, the child will default to joining the player
                        PawnGenerationRequest request = new PawnGenerationRequest(Props.staticPawnKind ?? mother?.kindDef ?? father?.kindDef ?? PawnKindDefOf.Colonist,
                            faction ?? Faction.OfPlayer, fixedLastName: RandomLastName(mother, father), allowDowned: true, forceNoIdeo: true,
                            forcedXenotype: Props.staticXenotype ?? XenotypeDefOf.Baseliner, developmentalStages: DevelopmentalStage.Newborn)
                        {
                            DontGivePreArrivalPathway = true
                        };

                        if (Props.staticXenotype == null || Props.staticXenotype.inheritable) request.ForcedEndogenes = Genes;
                        else request.ForcedXenogenes = Genes;

                        Pawn pawn = PawnGenerator.GeneratePawn(request);

                        if (Props.staticXenotype == null && (mother != null || father != null))
                            if (Props.xenotypeSource == XenoSource.Mother && mother != null)
                            {
                                pawn.genes.xenotypeName = mother.genes.xenotypeName;
                                pawn.genes.iconDef = mother.genes.iconDef;
                            }
                            else if (Props.xenotypeSource == XenoSource.Father && father != null)
                            {
                                pawn.genes.xenotypeName = father.genes.xenotypeName;
                                pawn.genes.iconDef = father.genes.iconDef;
                            }
                            else
                            {
                                if (GeneUtility.SameHeritableXenotype(mother, father) && mother?.genes?.UniqueXenotype == true)
                                {
                                    pawn.genes.xenotypeName = mother.genes.xenotypeName;
                                    pawn.genes.iconDef = mother.genes.iconDef;
                                }
                                if (TryGetInheritedXenotype(mother, father, out var xenotype))
                                {
                                    pawn.genes?.SetXenotypeDirect(xenotype);
                                }
                                else if (ShouldByHybrid(mother, father))
                                {
                                    pawn.genes.hybrid = true;
                                    pawn.genes.xenotypeName = "Hybrid".Translate();
                                }
                            }

                        if (map != null)
                        {
                            IntVec3? intVec = null;

                            if (Props.deleteOnFinalSpawn && numberToSpawn == 1 && spawnLeft == 0)
                                intVec = parent.Position;
                            else if (parent.InteractionCell.Walkable(map) && (alreadyUsedSpots.NullOrEmpty() || !alreadyUsedSpots.Contains(parent.InteractionCell)))
                            {
                                intVec = parent.InteractionCell;
                                alreadyUsedSpots.Add(parent.InteractionCell);
                            }
                            else intVec = CellFinder.RandomClosewalkCellNear(parent.InteractionCell, map, 1, delegate (IntVec3 cell)
                            {
                                if (!alreadyUsedSpots.NullOrEmpty() && alreadyUsedSpots.Contains(cell)) return false;
                                if (cell != parent.InteractionCell)
                                {
                                    Building building = map.edificeGrid[cell];
                                    if (building == null)
                                    {
                                        alreadyUsedSpots.Add(cell);
                                        return true;
                                    }

                                    if (building.def?.IsBed != true) alreadyUsedSpots.Add(cell);
                                    return building.def?.IsBed != true;
                                }
                                return false;
                            });
                            if (Props.filthOnCompletion != null) FilthMaker.TryMakeFilth(intVec.Value, map, ThingDefOf.Filth_AmnioticFluid, Props.filthPerSpawn.RandomInRange);

                            if (pawn.RaceProps.IsFlesh)
                            {
                                if (mother != null)
                                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
                                if (father != null)
                                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
                            }

                            if (pawn.playerSettings != null && mother?.playerSettings != null)
                                pawn.playerSettings.AreaRestrictionInPawnCurrentMap = mother.playerSettings.AreaRestrictionInPawnCurrentMap;

                            // It is unlikely that this will fail since it should be a spawned thing spitting the pawn out
                            if (!PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent, intVec))
                                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                        }
                        else caravan.AddPawn(pawn, true);

                        if (Props.bornThought)
                        {
                            mother?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.fatherBabyBornThought ?? ThoughtDefOf.BabyBorn, pawn);
                            father?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.motherBabyBornThought ?? ThoughtDefOf.BabyBorn, pawn);
                        }

                        if (Props.sendLetters && faction == Faction.OfPlayer)
                        {
                            ChoiceLetter_BabyBirth birthLetter = (ChoiceLetter_BabyBirth)LetterMaker.MakeLetter("EBSG_CompSpawnPawn".Translate(pawn.Label, EBSGUtilities.TranslateOrLiteral(Props.letterLabelNote)),
                                "EBSG_CompSpawnPawnText".Translate(parent.Label), LetterDefOf.BabyBirth, pawn);
                            birthLetter.Start();
                            Find.LetterStack.ReceiveLetter(birthLetter);
                        }

                        if (pawn.caller != null)
                            pawn.caller.DoCall();
                    }

                    if (spawnLeft == 0 && Props.deleteOnFinalSpawn)
                        parent.Destroy();

                    if (spawnLeft > 0)
                        ticksLeft = Props.completionTicks.RandomInRange; // Resets timer with the stored time reducing the next iteration
                }
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (Props.miscarriageThought && spawnLeft > 0)
            {
                mother?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.motherMiscarriageThought ?? ThoughtDefOf.Miscarried);
                father?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.fatherMiscarriageThought ?? ThoughtDefOf.PartnerMiscarried);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref mother, "mother");
            Scribe_References.Look(ref father, "father");
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref ticksLeft, "ticksLeft", 250);
            Scribe_Values.Look(ref spawnLeft, "spawnLeft", 1);
        }

        // Private methods from the pregnancy utility

        private static List<string> tmpLastNames = new List<string>(2);

        public static string RandomLastName(Pawn geneticMother, Pawn father)
        {
            tmpLastNames.Clear();
            if (geneticMother != null)
                tmpLastNames.Add(PawnNamingUtility.GetLastName(geneticMother));

            if (father != null)
                tmpLastNames.Add(PawnNamingUtility.GetLastName(father));

            if (tmpLastNames.Count == 0)
                return null;

            return tmpLastNames.RandomElement();
        }

        public static bool TryGetInheritedXenotype(Pawn mother, Pawn father, out XenotypeDef xenotype)
        {
            bool flag = mother?.genes != null;
            bool flag2 = father?.genes != null;
            if (flag && flag2 && mother.genes.Xenotype.inheritable && father.genes.Xenotype.inheritable && mother.genes.Xenotype == father.genes.Xenotype)
            {
                xenotype = mother.genes.Xenotype;
                return true;
            }
            if (flag && !flag2 && mother.genes.Xenotype.inheritable)
            {
                xenotype = mother.genes.Xenotype;
                return true;
            }
            if (flag2 && !flag && father.genes.Xenotype.inheritable)
            {
                xenotype = father.genes.Xenotype;
                return true;
            }
            xenotype = null;
            return false;
        }

        public static bool ShouldByHybrid(Pawn mother, Pawn father)
        {
            bool flag = mother?.genes != null;
            bool flag2 = father?.genes != null;
            if (flag && flag2)
            {
                if (mother.genes.hybrid && father.genes.hybrid)
                    return true;

                if (mother.genes.Xenotype.inheritable && father.genes.Xenotype.inheritable)
                    return true;

                bool num = flag && (mother.genes.Xenotype.inheritable || mother.genes.hybrid);
                bool flag3 = flag2 && (father.genes.Xenotype.inheritable || father.genes.hybrid);
                if (num || flag3)
                    return true;
            }
            if ((flag && !flag2 && mother.genes.hybrid) || (flag2 && !flag && father.genes.hybrid))
                return true;

            return false;
        }
    }
}

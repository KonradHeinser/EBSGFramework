using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

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
                    else switch (Props.xenotypeSource)
                    {
                        case XenoSource.Father when father != null:
                            genes = PregnancyUtility.GetInheritedGenes(father, null);
                            break;
                        case XenoSource.Mother when mother != null:
                            genes = PregnancyUtility.GetInheritedGenes(null, mother);
                            break;
                        default:
                            genes = PregnancyUtility.GetInheritedGenes(father, mother);
                            break;
                    }
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

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "RuinedByTemperature")
            {
                if (parent.Faction?.IsPlayer == true)
                    Messages.Message(parent.LabelShortCap + " : " + "RuinedByTemperature".Translate(), MessageTypeDefOf.NegativeEvent);
                if (Props.deleteOnFinalSpawn)
                    parent.Destroy();
                else
                    spawnLeft = 0;
            }
        }

        public override bool AllowStackWith(Thing other)
        {
            return false;
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);

            if (spawnLeft > 0 || spawnLeft == -1)
            {
                if (parent.GetComp<CompRefuelable>()?.HasFuel == false)
                    return;

                switch (parent)
                {
                    // Ensures plants and pawns are grown before they start creating new pawns
                    case Plant plant when plant.Growth < 1f:
                    case Pawn c when !c.RaceProps.IsMechanoid:
                        return;
                }

                ticksLeft -= delta;

                if (ticksLeft <= 0)
                {
                    Map map = parent.MapHeld;
                    Caravan caravan = null;
                    if (map == null && parent is Pawn carrier) caravan = carrier.GetCaravan();
                    if (map == null && caravan == null) return;

                    float fixedAge = 0f;

                    switch (Props.developmentalStage)
                    {
                        case DevelopmentalStage.Adult:
                            fixedAge = 18f;
                            break;
                        case DevelopmentalStage.Child:
                            fixedAge = 8f;
                            break;
                        case DevelopmentalStage.None:
                            return;
                        case DevelopmentalStage.Newborn:
                        case DevelopmentalStage.Baby:
                        default:
                            break;
                    }

                    int numberToSpawn = Props.spawnPerCompletion.RandomInRange;
                    List<IntVec3> alreadyUsedSpots = new List<IntVec3>();

                    if (spawnLeft != -1)
                    {
                        if (numberToSpawn > spawnLeft)
                            numberToSpawn = spawnLeft;
                        spawnLeft -= numberToSpawn;
                    }

                    numberToSpawn *= parent.stackCount;
                    for (int i = 0; i < numberToSpawn; i++)
                    {
                        // If the faction is somehow null, the child will default to joining the player
                        PawnGenerationRequest request = new PawnGenerationRequest(Props.staticPawnKind ?? mother?.kindDef ?? father?.kindDef ?? PawnKindDefOf.Colonist,
                            faction ?? Faction.OfPlayer, fixedLastName: RandomLastName(mother, father), allowDowned: true, forceNoIdeo: true, forceNoGear: true, fixedBiologicalAge: fixedAge,
                            fixedChronologicalAge: fixedAge, forcedXenotype: Props.staticXenotype ?? XenotypeDefOf.Baseliner, developmentalStages: Props.developmentalStage)
                        {
                            DontGivePreArrivalPathway = true,
                        };

                        if (Props.staticXenotype == null || Props.staticXenotype.inheritable) request.ForcedEndogenes = Genes;
                        else request.ForcedXenogenes = Genes;

                        Pawn pawn = PawnGenerator.GeneratePawn(request);

                        if (Props.staticXenotype == null && (mother != null || father != null))
                            switch (Props.xenotypeSource)
                            {
                                case XenoSource.Mother when mother != null:
                                    pawn.genes.xenotypeName = mother.genes.xenotypeName;
                                    pawn.genes.iconDef = mother.genes.iconDef;
                                    break;
                                case XenoSource.Father when father != null:
                                    pawn.genes.xenotypeName = father.genes.xenotypeName;
                                    pawn.genes.iconDef = father.genes.iconDef;
                                    break;
                                default:
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

                                    break;
                                }
                            }

                        if (map != null)
                        {
                            IntVec3? intVec;

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
                            pawn.babyNamingDeadline = Find.TickManager.TicksGame + 60000;
                            ChoiceLetter_BabyBirth birthLetter = (ChoiceLetter_BabyBirth)LetterMaker.MakeLetter("EBSG_CompSpawnPawn".Translate(pawn.Label, Props.letterLabelNote.TranslateOrFormat()),
                                "EBSG_CompSpawnPawnText".Translate(parent.Label), LetterDefOf.BabyBirth, pawn);
                            birthLetter.Start();
                            Find.LetterStack.ReceiveLetter(birthLetter);
                        }

                        pawn.caller?.DoCall();
                    }

                    if (spawnLeft == 0 && Props.deleteOnFinalSpawn)
                        parent.Destroy();
                    else if (spawnLeft > 0)
                        ticksLeft += Props.completionTicks.RandomInRange; // Resets timer with the stored time reducing the next iteration
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
            if ((flag && flag2 && mother.genes.Xenotype.inheritable && father.genes.Xenotype.inheritable && mother.genes.Xenotype == father.genes.Xenotype) 
                || (flag && !flag2 && mother.genes.Xenotype.inheritable))
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

                bool num = mother.genes.Xenotype.inheritable || mother.genes.hybrid;
                bool flag3 = father.genes.Xenotype.inheritable || father.genes.hybrid;
                if (num || flag3)
                    return true;
            }
            return (flag && !flag2 && mother.genes.hybrid) || (flag2 && !flag && father.genes.hybrid);
        }
    }
}

using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_SpawnHumanlike : HediffComp
    {
        HediffCompProperties_SpawnHumanlike Props => (HediffCompProperties_SpawnHumanlike)props;

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

        public override void CompPostMake()
        {
            base.CompPostMake();

            spawnLeft = Props.maxTotalSpawn;
            ticksLeft = Props.completionTicks.RandomInRange;
            mother = Pawn;
            if (faction == null) faction = mother?.Faction ?? father?.Faction;

            // When using give multiple hediffs, faction, father, and mother are set by the ability. 
            // The ability user is the "father", the target the "mother", and the faction is the same as the user's
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Props.linkedHediff != null && !EBSGUtilities.HasHediff(Pawn, Props.linkedHediff)) return;

            if (Props.onInterval && (spawnLeft > 0 || spawnLeft == -1) && parent.Severity >= Props.minSeverity && parent.Severity <= Props.maxSeverity)
            {
                ticksLeft--;

                if (ticksLeft <= 0)
                {
                    AssignLinkedFather();
                    SpawnPawns(Props.developmentalStage);

                    if (spawnLeft == 0 && Props.removeHediffOnFinalSpawn)
                        Pawn.health.RemoveHediff(parent);

                    if (spawnLeft == 0 && Props.killHostOnFinalSpawn)
                        Pawn.Kill(new DamageInfo(DamageDefOf.Cut, 99999f, 999f, -1f));

                    if (spawnLeft > 0)
                        ticksLeft = Props.completionTicks.RandomInRange; // Resets timer with the stored time reducing the next iteration
                }
            }
        }

        public override void Notify_PawnKilled()
        {
            if (spawnLeft != 0 && Props.onDeath && parent.Severity >= Props.minSeverity && parent.Severity <= Props.maxSeverity
                && (Props.linkedHediff == null || EBSGUtilities.HasHediff(Pawn, Props.linkedHediff)))
            {
                AssignLinkedFather();
                SpawnPawns(Props.devStageForRemovalOrDeath, Props.spawnRemainingOnRemovalOrDeath);
            }

            if (spawnLeft == 0 && Props.removeHediffOnFinalSpawn)
                Pawn.health.RemoveHediff(parent);
        }

        public override void CompPostPostRemoved()
        {
            if (!Pawn.Dead && spawnLeft != 0 && (Props.linkedHediff == null || EBSGUtilities.HasHediff(Pawn, Props.linkedHediff)))
            {
                AssignLinkedFather();
                if (Props.onRemoval && parent.Severity >= Props.minSeverity && parent.Severity <= Props.maxSeverity)
                    SpawnPawns(Props.devStageForRemovalOrDeath, Props.spawnRemainingOnRemovalOrDeath);

                if (Props.killHostOnRemoval)
                    Pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 99999f, 999f, -1f));
            }

            if (Props.miscarriageThought && spawnLeft > 0)
            {
                if (mother?.Faction == faction)
                    mother?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.motherMiscarriageThought ?? ThoughtDefOf.Miscarried);
                father?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.fatherMiscarriageThought ?? ThoughtDefOf.PartnerMiscarried);
            }
        }

        public void AssignLinkedFather()
        {
            if (Props.linkedHediff == null) return;
            Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.linkedHediff);
            if (hediff is HediffWithTarget targetHediff && targetHediff.target is Pawn parent) father = parent;
        }

        public void SpawnPawns(DevelopmentalStage developmentalStage, bool allRemaining = false)
        {
            Map map = Pawn.MapHeld;
            Caravan caravan = Pawn.GetCaravan();
            if (map == null && caravan == null) return;

            int numberToSpawn = Props.spawnPerCompletion.RandomInRange;
            List<IntVec3> alreadyUsedSpots = new List<IntVec3>();

            if (spawnLeft != -1)
            {
                if (allRemaining)
                    numberToSpawn = spawnLeft;
                else if (numberToSpawn > spawnLeft)
                    numberToSpawn = spawnLeft;
                spawnLeft -= numberToSpawn;
            }

            for (int i = 0; i < numberToSpawn; i++)
            {
                // If the faction is somehow null, the child will default to joining the player
                PawnGenerationRequest request = new PawnGenerationRequest(Props.staticPawnKind ?? mother?.kindDef ?? father?.kindDef ?? PawnKindDefOf.Colonist,
                    faction ?? Faction.OfPlayer, fixedLastName: RandomLastName(mother, father), allowDowned: true, forceNoIdeo: true,
                    forcedXenotype: Props.staticXenotype ?? XenotypeDefOf.Baseliner, developmentalStages: developmentalStage)
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

                    if (Pawn.Position.Walkable(map) && (alreadyUsedSpots.NullOrEmpty() || !alreadyUsedSpots.Contains(Pawn.Position)))
                    {
                        intVec = Pawn.Position;
                        alreadyUsedSpots.Add(Pawn.Position);
                    }
                    else intVec = CellFinder.RandomClosewalkCellNear(Pawn.Position, map, 1, delegate (IntVec3 cell)
                    {
                        if (!alreadyUsedSpots.NullOrEmpty() && alreadyUsedSpots.Contains(cell)) return false;
                        if (cell != Pawn.Position)
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

                    if (!PawnUtility.TrySpawnHatchedOrBornPawn(pawn, Pawn, intVec))
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }
                else caravan.AddPawn(pawn, true);

                if (Props.bornThought)
                {
                    if (mother?.Faction == faction)
                        mother?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.fatherBabyBornThought ?? ThoughtDefOf.BabyBorn, pawn);
                    father?.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.motherBabyBornThought ?? ThoughtDefOf.BabyBorn, pawn);
                }

                if (Props.sendLetters && faction == Faction.OfPlayer)
                {
                    pawn.babyNamingDeadline = Find.TickManager.TicksGame + 2500;
                    ChoiceLetter_BabyBirth birthLetter = (ChoiceLetter_BabyBirth)LetterMaker.MakeLetter("EBSG_CompSpawnPawn".Translate(pawn.Label, EBSGUtilities.TranslateOrLiteral(Props.letterLabelNote)),
                        "EBSG_CompSpawnPawnHediffText".Translate(Pawn.Label, EBSGUtilities.TranslateOrLiteral(Props.letterTextPawnDescription)), LetterDefOf.BabyBirth, pawn);
                    birthLetter.Start();
                    Find.LetterStack.ReceiveLetter(birthLetter);
                }

                if (pawn.caller != null)
                    pawn.caller.DoCall();
            }
        }


        public override void CompExposeData()
        {
            base.CompExposeData();
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

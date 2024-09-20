using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGCache_Component : GameComponent
    {
        public bool loaded;
        public int tick = 0;
        public int storyTellerAffinity;

        // Stat caches
        public List<StatDef> humanoidSlayingStats = new List<StatDef>();
        public List<StatDef> dryadSlayingStats = new List<StatDef>();
        public List<StatDef> insectSlayingStats = new List<StatDef>();
        public List<StatDef> animalSlayingStats = new List<StatDef>();
        public List<StatDef> mechanoidSlayingStats = new List<StatDef>();
        public List<StatDef> entitySlayingStats = new List<StatDef>();

        // Tabled due to complications, but kept as potential cache just in case
        public List<StatDef> bleedingSlayingStats = new List<StatDef>();
        public List<StatDef> nonBleedingSlayingStats = new List<StatDef>();

        // Terrain hediff comp cache
        private Dictionary<Pawn, Hediff> pawnTerrainComps;
        private Dictionary<Pawn, int> geneCountAtLastCache;
        private Dictionary<Pawn, float> cachedGeneMoodFactor;

        // Cached genes of interest
        public List<GeneDef> moodMultiplyingGenes = new List<GeneDef>();
        public List<GeneDef> dynamicResourceGenes = new List<GeneDef>();
        public List<GeneDef> hiddenWhenInactive = new List<GeneDef>();
        public List<GeneDef> noEquipment = new List<GeneDef>();
        public List<GeneDef> noApparel = new List<GeneDef>();
        public List<GeneDef> noWeapon = new List<GeneDef>();
        public List<GeneDef> equipRestricting = new List<GeneDef>();
        public List<GeneDef> grcGenes = new List<GeneDef>();

        // Cached needs of interest
        public List<NeedDef> murderousNeeds = new List<NeedDef>();

        // Cached hediffs of interest
        public List<HediffDef> explosiveAttackHediffs = new List<HediffDef>();

        private bool needNeedAlert = false;
        private bool checkedNeedAlert = false;

        private bool needComaAlert = false;
        private bool checkedComaAlert = false;

        private bool needRechargerJob = false;
        private bool checkedRechargerJob = false;

        // Other

        public List<Pawn> cachedHemogenicPawns = new List<Pawn>();

        public bool NeedComaAlert()
        {
            // Checks the def database to see if there are any needs that use displayLowAlert
            if (!checkedComaAlert)
            {
                foreach (NeedDef need in DefDatabase<NeedDef>.AllDefsListForReading)
                {
                    if (need.HasModExtension<EBSGExtension>())
                    {
                        EBSGExtension extension = need.GetModExtension<EBSGExtension>();
                        if (extension.displayLowAlert)
                        {
                            needComaAlert = true;
                            break;
                        }
                    }
                }

                checkedComaAlert = true;
            }

            return needComaAlert;
        }

        public bool NeedNeedAlert()
        {
            // Checks the def database to see if there are any needs that use displayLowAlert
            if (!checkedNeedAlert)
            {
                foreach (NeedDef need in DefDatabase<NeedDef>.AllDefsListForReading)
                {
                    if (need.HasModExtension<EBSGExtension>())
                    {
                        EBSGExtension extension = need.GetModExtension<EBSGExtension>();
                        if (extension.displayLowAlert)
                        {
                            needNeedAlert = true;
                            break;
                        }
                    }
                }

                checkedNeedAlert = true;
            }

            return needNeedAlert;
        }

        public bool NeedRechargerJob()
        {
            if (!checkedRechargerJob)
            {
                foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (thing.thingClass == typeof(Building_PawnNeedCharger))
                    {
                        needRechargerJob = true;
                        break;
                    }
                }

                checkedRechargerJob = true;
            }

            return needRechargerJob;
        }

        public bool NeedEquipRestrictGeneCheck()
        {
            return !(noEquipment.NullOrEmpty() && noWeapon.NullOrEmpty() && noApparel.NullOrEmpty() && equipRestricting.NullOrEmpty());
        }

        // Gene result caching

        public float GetGeneMoodFactor(Pawn pawn)
        {
            if (moodMultiplyingGenes.NullOrEmpty() || pawn.genes == null || pawn.genes.GenesListForReading.NullOrEmpty()) return 1f;

            // Regularly tries to do a recheck on the off-chance that genes have changed without changing count
            if (geneCountAtLastCache.NullOrEmpty() || !geneCountAtLastCache.ContainsKey(pawn) || pawn.genes.GenesListForReading.Count != geneCountAtLastCache[pawn]
                    || pawn.IsHashIntervalTick(60000) || cachedGeneMoodFactor.NullOrEmpty() || !cachedGeneMoodFactor.ContainsKey(pawn))
            {
                float num = 1f;

                foreach (GeneDef gene in moodMultiplyingGenes)
                {
                    if (EBSGUtilities.HasRelatedGene(pawn, gene))
                    {
                        EBSGExtension extension = gene.GetModExtension<EBSGExtension>();
                        if (extension != null)
                        {
                            if (extension.universalMoodFactor == 0)
                            {
                                num = 0;
                                break;
                            }
                            num *= extension.universalMoodFactor;
                        }
                    }
                }

                if (geneCountAtLastCache.ContainsKey(pawn))
                    geneCountAtLastCache[pawn] = pawn.genes.GenesListForReading.Count;
                else
                    geneCountAtLastCache.Add(pawn, pawn.genes.GenesListForReading.Count);

                if (cachedGeneMoodFactor.ContainsKey(pawn))
                    cachedGeneMoodFactor[pawn] = num;
                else
                    cachedGeneMoodFactor.Add(pawn, num);
            }

            return cachedGeneMoodFactor[pawn];
        }

        public void CachePawnWithGene(Pawn pawn)
        {
            if (pawn.genes == null || pawn.genes.GenesListForReading.NullOrEmpty()) return;
            if (pawn.genes.GetFirstGeneOfType<Gene_Hemogen>() != null)
                cachedHemogenicPawns.Add(pawn);
        }

        // Terrain cost cache

        public void RegisterTerrainPawn(Pawn pawn, Hediff hediff)
        {
            if (!pawnTerrainComps.ContainsKey(pawn))
                pawnTerrainComps.Add(pawn, hediff);
            else
                pawnTerrainComps[pawn] = hediff;
        }

        public void DeRegisterTerrainPawn(Pawn pawn)
        {
            if (!pawnTerrainComps.NullOrEmpty() && pawnTerrainComps.ContainsKey(pawn))
                pawnTerrainComps.Remove(pawn);
        }

        public bool GetPawnTerrainComp(Pawn pawn, out HediffCompProperties_TerrainCostOverride comp)
        {
            // Only the first one applies to the pawn

            bool flag = true;

            if (pawnTerrainComps.NullOrEmpty() || !pawnTerrainComps.ContainsKey(pawn) || !EBSGUtilities.HasHediff(pawn, pawnTerrainComps[pawn].def) || pawnTerrainComps[pawn].TryGetComp<HediffComp_TerrainCostOverride>() == null)
            {
                DeRegisterTerrainPawn(pawn);
                flag = false;

                if (pawn.health != null && !pawn.health.hediffSet.hediffs.NullOrEmpty())
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                        if (hediff.TryGetComp<HediffComp_TerrainCostOverride>() != null)
                        {
                            RegisterTerrainPawn(pawn, hediff);
                            flag = true;
                            break;
                        }
                }
            }

            if (flag)
            {
                comp = pawnTerrainComps[pawn].TryGetComp<HediffComp_TerrainCostOverride>().Props;
                return true;
            }

            comp = null;
            return false;
        }

        // Initializers

        public override void StartedNewGame()
        {
            Initialize();
        }

        public override void LoadedGame()
        {
            Initialize();
        }

        // Storyteller affinity uses this because storytellers don't have an expose data apparently

        public int UpdateAffinity(int offset, int adoredAffinity, int despisedAffinity)
        {
            storyTellerAffinity += offset;
            if (storyTellerAffinity > adoredAffinity) storyTellerAffinity = adoredAffinity;
            else if (storyTellerAffinity < despisedAffinity) storyTellerAffinity = despisedAffinity;

            return storyTellerAffinity;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref storyTellerAffinity, "storyTellerAffinity", 0);
        }

        public EBSGCache_Component(Game game)
        {
            loaded = false;

            pawnTerrainComps = new Dictionary<Pawn, Hediff>();
            geneCountAtLastCache = new Dictionary<Pawn, int>();
            cachedGeneMoodFactor = new Dictionary<Pawn, float>();
        }

        public void Initialize()
        {
            // Clears all caches every time
            loaded = true;

            pawnTerrainComps = new Dictionary<Pawn, Hediff>();
            geneCountAtLastCache = new Dictionary<Pawn, int>();
            cachedGeneMoodFactor = new Dictionary<Pawn, float>();

            RebuildCaches();
            EBSG_Settings.BuildThinkTreeSettings(); // Ensures think tree settings contain all that they should
        }

        // Rather than saving them, they are just cached like this to minimize the risk of weird save fuckery caused by mod changes
        public void RebuildCaches()
        {
            cachedHemogenicPawns = new List<Pawn>();

            foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
                CachePawnWithGene(pawn);

            CacheGenesOfInterest();
            CacheStatsOfInterest();
            CacheNeedsOfInterest();
            CacheHediffsOfInterest();
        }

        private void CacheGenesOfInterest()
        {
            moodMultiplyingGenes = new List<GeneDef>();
            hiddenWhenInactive = new List<GeneDef>();
            dynamicResourceGenes = new List<GeneDef>();
            noEquipment = new List<GeneDef>();
            noApparel = new List<GeneDef>();
            noWeapon = new List<GeneDef>();
            equipRestricting = new List<GeneDef>();
            grcGenes = new List<GeneDef>();

            foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
            {
                if (gene.HasModExtension<EBSGExtension>())
                {
                    EBSGExtension extension = gene.GetModExtension<EBSGExtension>();

                    if (extension.universalMoodFactor != 1)
                        moodMultiplyingGenes.Add(gene);

                    if (extension.hideInGeneTabWhenInactive)
                        hiddenWhenInactive.Add(gene);
                }
                if (gene.geneClass == typeof(ResourceGene))
                    dynamicResourceGenes.Add(gene);
                if (gene.HasModExtension<EquipRestrictExtension>())
                {
                    EquipRestrictExtension equipRestrict = gene.GetModExtension<EquipRestrictExtension>();

                    if (equipRestrict.noEquipment || (equipRestrict.noWeapons && equipRestrict.noApparel))
                        noEquipment.Add(gene);
                    else if (equipRestrict.noApparel)
                        noApparel.Add(gene);
                    else if (equipRestrict.noWeapons)
                        noWeapon.Add(gene);
                    else
                        equipRestricting.Add(gene);
                }
                if (gene.HasModExtension<GRCExtension>())
                    grcGenes.Add(gene);
            }
        }

        private void CacheStatsOfInterest()
        {
            humanoidSlayingStats = new List<StatDef>();
            dryadSlayingStats = new List<StatDef>();
            insectSlayingStats = new List<StatDef>();
            animalSlayingStats = new List<StatDef>();
            mechanoidSlayingStats = new List<StatDef>();
            entitySlayingStats = new List<StatDef>();
            // bleedingSlayingStats = new List<StatDef>();
            // nonBleedingSlayingStats = new List<StatDef>();

            foreach (StatDef stat in DefDatabase<StatDef>.AllDefs)
            {
                if (stat.HasModExtension<EBSGDamageExtension>())
                {
                    EBSGDamageExtension extension = stat.GetModExtension<EBSGDamageExtension>();
                    if (extension.allowHumanlikes) humanoidSlayingStats.Add(stat);
                    if (extension.allowDryads) dryadSlayingStats.Add(stat);
                    if (extension.allowInsects) insectSlayingStats.Add(stat);
                    if (extension.allowAnimals) animalSlayingStats.Add(stat);
                    if (extension.allowMechanoids) mechanoidSlayingStats.Add(stat);
                    if (extension.allowEntities) entitySlayingStats.Add(stat);
                    // if (extension.allowBleedable) bleedingSlayingStats.Add(stat);
                    // if (extension.allowNonBleedable) nonBleedingSlayingStats.Add(stat);
                }
            }
        }

        private void CacheNeedsOfInterest()
        {
            murderousNeeds = new List<NeedDef>();

            foreach (NeedDef need in DefDatabase<NeedDef>.AllDefs)
            {
                if (need.needClass == typeof(Need_Murderous))
                    murderousNeeds.Add(need);
            }
        }

        private void CacheHediffsOfInterest()
        {
            explosiveAttackHediffs = new List<HediffDef>();

            foreach (HediffDef hediff in DefDatabase<HediffDef>.AllDefs)
            {
                if (hediff.comps.NullOrEmpty()) continue;
                if (hediff.HasComp(typeof(HediffComp_ExplodingAttacks)) || hediff.HasComp(typeof(HediffComp_ExplodingRangedAttacks))
                    || hediff.HasComp(typeof(HediffComp_ExplodingMeleeAttacks))) explosiveAttackHediffs.Add(hediff);
            }
        }

        // Post load

        public override void GameComponentTick()
        {
            tick++;
            if (tick % 200 == 0)
            {
                if (!cachedHemogenicPawns.NullOrEmpty())
                {
                    List<Pawn> purgePawns = new List<Pawn>();
                    foreach (Pawn pawn in cachedHemogenicPawns)
                    {
                        if (pawn.genes == null || pawn.genes.GenesListForReading.NullOrEmpty() || pawn.genes.GetFirstGeneOfType<Gene_Hemogen>() == null)
                            purgePawns.Add(pawn);
                        else if (!pawn.Dead)
                        {
                            float baseAmount = 1f;
                            Gene_Deathrest deathrest = pawn.genes.GetFirstGeneOfType<Gene_Deathrest>();
                            if (deathrest != null && !deathrest.BoundBuildings.NullOrEmpty())
                            {
                                foreach (Thing thing in deathrest.BoundBuildings)
                                    if (thing.HasComp<CompDeathrestBindable>())
                                    {
                                        CompDeathrestBindable bindable = thing.TryGetComp<CompDeathrestBindable>();
                                        baseAmount += bindable.Props.hemogenLimitOffset;
                                    }
                            }
                            Gene_Hemogen gene = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
                            gene.SetMax((baseAmount + pawn.GetStatValue(EBSGDefOf.EBSG_HemogenMaxOffset)) * pawn.GetStatValue(EBSGDefOf.EBSG_HemogenMaxFactor));
                        }
                    }
                    if (!purgePawns.NullOrEmpty())
                        foreach (Pawn pawn in purgePawns)
                            cachedHemogenicPawns.Remove(pawn);
                }
            }
        }
    }
}

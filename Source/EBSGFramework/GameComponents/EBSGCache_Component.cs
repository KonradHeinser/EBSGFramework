﻿using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGCache_Component : GameComponent
    {
        public bool loaded;
        public int tick = 0;
        public int storyTellerAffinity;

        // Hot Caches (caches only made mid-game when called)

        private Dictionary<string, List<TerrainDef>> viableGatheringSpots;
        private Dictionary<string, List<BiomeDef>> viableGatheringBiomes;

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

        private bool needNeedAlert = false;
        private bool checkedNeedAlert = false;

        private bool needRechargerJob = false;
        private bool checkedRechargerJob = false;

        // Hot Caching

        public List<TerrainDef> ViableGatheringSpots(ThingDef thing)
        {
            if (viableGatheringSpots.NullOrEmpty()) viableGatheringSpots = new Dictionary<string, List<TerrainDef>>();
            else if (viableGatheringSpots.ContainsKey(thing.defName)) return viableGatheringSpots[thing.defName];

            if (thing == null || thing.comps.NullOrEmpty() || !thing.HasComp(typeof(CompGatherSpot))) return null;

            CompProperties_GathererSpot gatherProps = thing.GetCompProperties<CompProperties_GathererSpot>();

            List<TerrainDef> cachedViableTerrain = new List<TerrainDef>();
            if (gatherProps.options.NullOrEmpty())
            {
                viableGatheringSpots.Add(thing.defName, cachedViableTerrain);
                return cachedViableTerrain;
            }

            bool forbiddenOnly = true;

            foreach (GatherOption option in gatherProps.options)
            {
                if (!option.viableTerrain.NullOrEmpty())
                {
                    forbiddenOnly = false;
                    foreach (TerrainDef terrain in option.viableTerrain)
                        if (!cachedViableTerrain.Contains(terrain)) cachedViableTerrain.Add(terrain);
                }
                if (forbiddenOnly && option.forbiddenTerrain.NullOrEmpty()) // If at least one of them has no forbidden terrains, then the forbidden check code is just a performance waster, and it will be assumed that all terrain works
                    forbiddenOnly = false;
            }

            if (cachedViableTerrain.NullOrEmpty())
                cachedViableTerrain = new List<TerrainDef>(DefDatabase<TerrainDef>.AllDefs);

            if (forbiddenOnly)
            {
                // This is checking for any terrain that all of them forbid by comparing the list of the first one with every other list
                foreach (TerrainDef terrain in gatherProps.options[0].forbiddenTerrain)
                {
                    bool flag = true; // If this remains true then it is a terrain forbidden by all options
                    foreach (GatherOption option in gatherProps.options)
                        if (!option.forbiddenTerrain.Contains(terrain))
                        {
                            flag = false;
                            break;
                        }
                    if (flag) cachedViableTerrain.Remove(terrain);
                }
            }

            viableGatheringSpots.Add(thing.defName, cachedViableTerrain);
            return cachedViableTerrain;
        }

        public List<BiomeDef> ViableGatheringBiomes(ThingDef thing)
        {
            if (viableGatheringSpots.NullOrEmpty()) viableGatheringBiomes = new Dictionary<string, List<BiomeDef>>();
            else if (viableGatheringSpots.ContainsKey(thing.defName)) return viableGatheringBiomes[thing.defName];

            if (thing == null || thing.comps.NullOrEmpty() || !thing.HasComp(typeof(CompGatherSpot))) return null;

            CompProperties_GathererSpot gatherProps = thing.GetCompProperties<CompProperties_GathererSpot>();

            List<BiomeDef> cachedViableBiome = new List<BiomeDef>();
            if (gatherProps.options.NullOrEmpty())
            {
                viableGatheringBiomes.Add(thing.defName, cachedViableBiome);
                return cachedViableBiome;
            }

            bool forbiddenOnly = true;

            foreach (GatherOption option in gatherProps.options)
            {
                if (!option.validBiomes.NullOrEmpty())
                {
                    forbiddenOnly = false;
                    foreach (BiomeDef biome in option.validBiomes)
                        if (!cachedViableBiome.Contains(biome)) cachedViableBiome.Add(biome);
                }
                if (forbiddenOnly && option.forbiddenBiomes.NullOrEmpty()) // If at least one of them has no forbidden biomes, then the forbidden check code is just a performance waster, and it will be assumed that all biome works
                    forbiddenOnly = false;
            }

            if (cachedViableBiome.NullOrEmpty())
                cachedViableBiome = new List<BiomeDef>(DefDatabase<BiomeDef>.AllDefs);

            if (forbiddenOnly)
            {
                // This is checking for any biome that all of them forbid by comparing the list of the first one with every other list
                foreach (BiomeDef biome in gatherProps.options[0].forbiddenBiomes)
                {
                    bool flag = true; // If this remains true then it is a biome forbidden by all options
                    foreach (GatherOption option in gatherProps.options)
                        if (!option.forbiddenBiomes.Contains(biome))
                        {
                            flag = false;
                            break;
                        }
                    if (flag) cachedViableBiome.Remove(biome);
                }
            }

            viableGatheringBiomes.Add(thing.defName, cachedViableBiome);
            return cachedViableBiome;
        }

        // Other

        public List<Pawn> cachedHemogenicPawns = new List<Pawn>();

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
                    if (pawn.genes.HasGene(gene))
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
        }

        // Rather than saving them, they are just cached like this to minimize the risk of weird save fuckery caused by mod changes
        public void RebuildCaches()
        {
            cachedHemogenicPawns = new List<Pawn>();

            foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
                CachePawnWithGene(pawn);

            CacheGenesOfInterest();
            CacheStatsOfInterest();
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

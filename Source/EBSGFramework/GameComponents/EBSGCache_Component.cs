using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class EBSGCache_Component : GameComponent
    {
        public bool loaded;
        public int tick = 0;
        public int storyTellerAffinity;

        // Ability Caches
        public List<AbilityDef> reloadableAbilities = new List<AbilityDef>();
        public List<ThingDef> abilityFuel = new List<ThingDef>();

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
        public List<GeneDef> fertilityChangingGenes = new List<GeneDef>();
        public List<GeneDef> moodMultiplyingGenes = new List<GeneDef>();
        public List<GeneDef> dynamicResourceGenes = new List<GeneDef>();
        public List<GeneDef> hiddenWhenInactive = new List<GeneDef>();
        public List<GeneDef> skillChanging = new List<GeneDef>();

        public List<GeneDef> desiccatedHeads = new List<GeneDef>();
        public List<GeneDef> desiccatedBodies = new List<GeneDef>();
        public List<GeneDef> ageBasedHeads = new List<GeneDef>();
        public List<GeneDef> ageBasedBodies = new List<GeneDef>();

        public List<GeneDef> noEquipment = new List<GeneDef>();
        public List<GeneDef> noApparel = new List<GeneDef>();
        public List<GeneDef> noWeapon = new List<GeneDef>();
        public List<GeneDef> equipRestricting = new List<GeneDef>();

        public List<GeneDef> idgGenes = new List<GeneDef>();
        public List<GeneDef> grcGenes = new List<GeneDef>();

        public List<GeneDef> bloodReplacingGenes = new List<GeneDef>();
        public List<GeneDef> bloodSmearReplacingGenes = new List<GeneDef>();

        public List<GeneDef> pregnancyReplacingGenes = new List<GeneDef>();
        public List<GeneDef> lovinAddinGenes = new List<GeneDef>();
        public List<GeneDef> butcherProductGenes = new List<GeneDef>();
        public List<GeneDef> leatherProductGenes = new List<GeneDef>();

        public List<GeneDef> forbidFoods = new List<GeneDef>();
        public List<GeneDef> restrictFoods = new List<GeneDef>();
        public List<GeneDef> nonIngestibleFoods = new List<GeneDef>();
        public List<GeneDef> noStandardFoods = new List<GeneDef>();
        public List<GeneDef> foodTypeOverrides = new List<GeneDef>();

        public List<GeneDef> outgoingDamageStatGenes = new List<GeneDef>();
        public List<GeneDef> incomingDamageStatGenes = new List<GeneDef>();

        public bool NeedEatPatch
        {
            get
            {
                return !forbidFoods.NullOrEmpty() || !restrictFoods.NullOrEmpty() || !noStandardFoods.NullOrEmpty();
            }
        }

        public bool FoodTypeOverride
        {
            get
            {
                return !noStandardFoods.NullOrEmpty() || !foodTypeOverrides.NullOrEmpty();
            }
        }

        // Cached needs of interest
        public List<NeedDef> murderousNeeds = new List<NeedDef>();

        // Cached hediffs of interest
        public List<HediffDef> explosiveAttackHediffs = new List<HediffDef>();
        public List<HediffDef> skillChangeHediffs = new List<HediffDef>();
        public List<HediffDef> shieldHediffs = new List<HediffDef>();

        private bool needNeedAlert = false;
        private bool checkedNeedAlert = false;

        private bool needComaAlert = false;
        private bool checkedComaNeeds = false;

        private bool needRechargerJob = false;
        private bool checkedRechargerJob = false;

        // Cached things of interest
        public bool needEquippableAbilityPatches = false;
        public List<ThingDef> shieldEquipment = new List<ThingDef>();

        // Other

        public List<Pawn> cachedHemogenicPawns = new List<Pawn>();

        public bool ComaNeedsExist()
        {
            // Checks the def database to see if there are any needs that use displayLowAlert
            if (!checkedComaNeeds)
            {
                foreach (NeedDef need in DefDatabase<NeedDef>.AllDefsListForReading)
                    if (need.needClass == typeof(Need_ComaGene))
                    {
                        needComaAlert = true;
                        break;
                    }
                checkedComaNeeds = true;
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
                    if (pawn.HasRelatedGene(gene))
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

            if (pawnTerrainComps.NullOrEmpty() || !pawnTerrainComps.ContainsKey(pawn) || !pawn.HasHediff(pawnTerrainComps[pawn].def) || pawnTerrainComps[pawn].TryGetComp<HediffComp_TerrainCostOverride>() == null)
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
            CacheAbilitiesOfInterest();
            CacheStatsOfInterest();
            CacheNeedsOfInterest();
            CacheHediffsOfInterest();
            CacheThingsOfInterest();
        }

        private void CacheAbilitiesOfInterest()
        {
            reloadableAbilities = new List<AbilityDef>();
            abilityFuel = new List<ThingDef>();

            foreach (AbilityDef ability in DefDatabase<AbilityDef>.AllDefs)
            {
                if (!ability.comps.NullOrEmpty())
                    foreach (AbilityCompProperties prop in ability.comps)
                        if (prop is CompProperties_AbilityReloadable reloadable)
                        {
                            reloadableAbilities.Add(ability);
                            if (!abilityFuel.Contains(reloadable.ammoDef))
                                abilityFuel.Add(reloadable.ammoDef);
                        }
            }
        }

        private void CacheGenesOfInterest()
        {
            fertilityChangingGenes = new List<GeneDef>();
            moodMultiplyingGenes = new List<GeneDef>();
            hiddenWhenInactive = new List<GeneDef>();
            skillChanging = new List<GeneDef>();

            desiccatedHeads = new List<GeneDef>();
            desiccatedBodies = new List<GeneDef>();
            ageBasedHeads = new List<GeneDef>();
            ageBasedBodies = new List<GeneDef>();

            dynamicResourceGenes = new List<GeneDef>();
            noEquipment = new List<GeneDef>();
            noApparel = new List<GeneDef>();
            noWeapon = new List<GeneDef>();
            equipRestricting = new List<GeneDef>();

            idgGenes = new List<GeneDef>();
            grcGenes = new List<GeneDef>();
            
            bloodReplacingGenes = new List<GeneDef>();
            bloodSmearReplacingGenes = new List<GeneDef>();
            
            pregnancyReplacingGenes = new List<GeneDef>();
            lovinAddinGenes = new List<GeneDef>();
            butcherProductGenes = new List<GeneDef>();
            leatherProductGenes = new List<GeneDef>();
            outgoingDamageStatGenes = new List<GeneDef>();
            incomingDamageStatGenes = new List<GeneDef>();

            foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
            {
                if (gene.HasModExtension<FertilityByGenderAgeExtension>())
                    fertilityChangingGenes.Add(gene);
                if (gene.geneClass == typeof(Gene_Dependency))
                    idgGenes.Add(gene);
                if (gene.HasModExtension<GRCExtension>())
                    grcGenes.Add(gene);
                if (gene.geneClass == typeof(ResourceGene))
                    dynamicResourceGenes.Add(gene);

                if (gene.HasModExtension<EBSGExtension>())
                {
                    EBSGExtension extension = gene.GetModExtension<EBSGExtension>();

                    if (extension.universalMoodFactor != 1)
                        moodMultiplyingGenes.Add(gene);

                    if (extension.hideInGeneTabWhenInactive)
                        hiddenWhenInactive.Add(gene);

                    if (!extension.skillChanges.NullOrEmpty())
                        skillChanging.Add(gene);

                    if (extension.bloodDropChance != 1 || extension.bloodReplacement != null)
                        bloodReplacingGenes.Add(gene);

                    if (extension.bloodSmearDropChance != 1 || extension.bloodSmearReplacement != null)
                        bloodReplacingGenes.Add(gene);
                }

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

                if (gene.HasModExtension<FoodExtension>())
                {
                    FoodExtension foodExtension = gene.GetModExtension<FoodExtension>();

                    if (!foodExtension.forbiddenFoods.NullOrEmpty()) forbidFoods.Add(gene);
                    if (!foodExtension.allowedFoods.NullOrEmpty()) restrictFoods.Add(gene);
                    if (!foodExtension.nonIngestibleFoods.NullOrEmpty()) restrictFoods.Add(gene);
                    if (foodExtension.noStandardFood) noStandardFoods.Add(gene);
                    if (foodExtension.foodTypeOverride != FoodTypeFlags.None) foodTypeOverrides.Add(gene);
                }

                EBSGBodyExtension bodyExtension = gene.GetModExtension<EBSGBodyExtension>();
                if (bodyExtension != null)
                {
                    if (bodyExtension.desHead != null || bodyExtension.desChildHead != null)
                        desiccatedHeads.Add(gene);
                    if (bodyExtension.desBody != null || bodyExtension.desFat != null || bodyExtension.desHulk != null || bodyExtension.desThin != null
                        || bodyExtension.desFemale != null || bodyExtension.desMale != null || bodyExtension.desChild != null)
                        desiccatedBodies.Add(gene);
                    if (!bodyExtension.ageGraphics.NullOrEmpty())
                    {
                        if (!bodyExtension.ageGraphics.Where((arg) => arg.childHead != null || arg.head != null).EnumerableNullOrEmpty())
                            ageBasedHeads.Add(gene);
                        if (!bodyExtension.ageGraphics.Where((arg) => arg.child != null || arg.body != null || arg.female != null || arg.male != null ||
                            arg.hulk != null || arg.thin != null || arg.fat != null).EnumerableNullOrEmpty())
                            ageBasedBodies.Add(gene);
                    }
                }

                if (gene.HasModExtension<PregnancyReplacerExtension>())
                    pregnancyReplacingGenes.Add(gene);

                if (gene.HasModExtension<PostLovinThingsExtension>())
                    lovinAddinGenes.Add(gene);

                ButcherProductExtension butcher = gene.GetModExtension<ButcherProductExtension>();
                if (butcher != null)
                {
                    butcherProductGenes.Add(gene);
                    if (butcher.leatherReplacement != null)
                        leatherProductGenes.Add(gene);
                }

                DamageModifyingStatsExtension damageStats = gene.GetModExtension<DamageModifyingStatsExtension>();
                if (damageStats != null)
                {
                    if (damageStats.Outgoing)
                        outgoingDamageStatGenes.Add(gene);
                    if (damageStats.Incoming)
                        incomingDamageStatGenes.Add(gene);
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
            skillChangeHediffs = new List<HediffDef>();
            shieldHediffs = new List<HediffDef>();

            foreach (HediffDef hediff in DefDatabase<HediffDef>.AllDefs)
            {
                if (hediff.comps.NullOrEmpty()) continue;
                foreach (HediffCompProperties comp in hediff.comps)
                {
                    if (comp is HediffCompProperties_ExplodingAttacks || comp is HediffCompProperties_ExplodingRangedAttacks || comp is HediffCompProperties_ExplodingMeleeAttacks)
                    {
                        explosiveAttackHediffs.Add(hediff);
                        continue;
                    }
                    if (comp is HediffCompProperties_TemporarySkillChange skillChange)
                    {
                        if (!skillChange.skillChanges.Where((arg) => arg.skillChange != new IntRange(0, 0)).EnumerableNullOrEmpty())
                            skillChangeHediffs.Add(hediff);
                        continue;
                    }
                    if (comp is HediffCompProperties_Shield)
                    {
                        shieldHediffs.Add(hediff);
                        continue;
                    }
                }
            }
        }

        private void CacheThingsOfInterest()
        {
            shieldEquipment = new List<ThingDef>();

            foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs)
            {
                needEquippableAbilityPatches |= thing.HasComp<CompAbilityLimitedCharges>();
                
                if (thing.HasComp<CompShieldEquipment>())
                    shieldEquipment.Add(thing);
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

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

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

        public RestrictType GloballyRestrictedEquipment(Pawn pawn, Thing thing, out string source)
        {
            if (!noEquipment.NullOrEmpty())
                if (pawn.PawnHasAnyOfGenes(out var gene, noEquipment))
                {
                    source = gene.LabelCap;
                    return RestrictType.All;
                }
            
            if (thing.def.IsWeapon && !noWeapon.NullOrEmpty())
                if (pawn.PawnHasAnyOfGenes(out var gene, noWeapon))
                {
                    source = gene.LabelCap;
                    return RestrictType.Weapon;
                }
            
            if (thing.def.IsApparel && !noApparel.NullOrEmpty())
                if (pawn.PawnHasAnyOfGenes(out var gene, noApparel))
                {
                    source = gene.LabelCap;
                    return RestrictType.Apparel;
                }
            
            source = null;
            return RestrictType.None;
        }
        
        public List<GeneDef> equipRestricting = new List<GeneDef>();

        public List<GeneDef> idgGenes = new List<GeneDef>();
        public List<GeneDef> grcGenes = new List<GeneDef>();

        public List<GeneDef> pregnancyReplacingGenes = new List<GeneDef>();
        public List<GeneDef> lovinAddinGenes = new List<GeneDef>();
        public List<GeneDef> partnerLovinMemoryReplacer = new List<GeneDef>();
        public List<GeneDef> butcherProductGenes = new List<GeneDef>();
        public List<GeneDef> leatherProductGenes = new List<GeneDef>();

        public List<GeneDef> outgoingDamageStatGenes = new List<GeneDef>();
        public List<GeneDef> incomingDamageStatGenes = new List<GeneDef>();
        public List<GeneDef> downedMemoryGenes = new List<GeneDef>();
        public Dictionary<GeneDef, HistoryEventDef> propagateEvents = new Dictionary<GeneDef, HistoryEventDef>();

        // Cached needs of interest
        public List<NeedDef> murderousNeeds = new List<NeedDef>();

        // Cached hediffs of interest
        public List<HediffDef> explosiveAttackHediffs = new List<HediffDef>();
        public List<HediffDef> skillChangeHediffs = new List<HediffDef>();
        public List<HediffDef> shieldHediffs = new List<HediffDef>();
        public List<HediffDef> nameColorHediffs = new List<HediffDef>();

        private bool needNeedAlert;
        private bool checkedNeedAlert;

        private bool needComaAlert;
        private bool checkedComaNeeds;

        private bool needRechargerJob;
        private bool checkedRechargerJob;

        // Cached things of interest
        public bool needEquippableAbilityPatches;
        public List<ThingDef> shieldEquipment = new List<ThingDef>();
        public List<WeaponTraitDef> meleeWeaponTraits = new List<WeaponTraitDef>();
        public List<WeaponTraitDef> projectileOverrideTraits = new List<WeaponTraitDef>();
        
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
        
        // Name Color Caching

        public Dictionary<Pawn, Color> pawnNameColors = new Dictionary<Pawn, Color>();

        public Color? GetPawnNameColor(Pawn pawn)
        {
            Color? result = null;

            if (!nameColorHediffs.NullOrEmpty())
            {
                if ((pawn.IsHashIntervalTick(250) || !pawnNameColors.ContainsKey(pawn)))
                {
                    if (pawn.PawnHasAnyOfHediffs(nameColorHediffs, out Hediff match))
                        pawnNameColors[pawn] = match.TryGetComp<HediffComp_NameColor>().Props.color;
                    else
                        pawnNameColors.Remove(pawn);
                } 

                if (pawnNameColors.TryGetValue(pawn, out var color))
                    result = color;
            }
            
            return result;
        }

        // Gene result caching

        public float GetGeneMoodFactor(Pawn pawn)
        {
            if (moodMultiplyingGenes.NullOrEmpty() || pawn.genes?.GenesListForReading.NullOrEmpty() != false) 
                return 1f;

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

                geneCountAtLastCache[pawn] = pawn.genes.GenesListForReading.Count;

                cachedGeneMoodFactor[pawn] = num;
            }

            return cachedGeneMoodFactor[pawn];
        }

        // Terrain cost cache

        public void RegisterTerrainPawn(Pawn pawn, Hediff hediff)
        {
            pawnTerrainComps[pawn] = hediff;
        }

        public void DeRegisterTerrainPawn(Pawn pawn)
        {
            if (!pawnTerrainComps.NullOrEmpty())
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
            pawnNameColors = new Dictionary<Pawn, Color>();

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
            
            pregnancyReplacingGenes = new List<GeneDef>();
            lovinAddinGenes = new List<GeneDef>();
            partnerLovinMemoryReplacer = new List<GeneDef>();
            butcherProductGenes = new List<GeneDef>();
            leatherProductGenes = new List<GeneDef>();
            outgoingDamageStatGenes = new List<GeneDef>();
            incomingDamageStatGenes = new List<GeneDef>();
            downedMemoryGenes = new List<GeneDef>();

            propagateEvents = new Dictionary<GeneDef, HistoryEventDef>();

            if (EBSGDefOf.EBSG_Recorder?.geneEvents.NullOrEmpty() == false)
                foreach (var geneEvent in EBSGDefOf.EBSG_Recorder.geneEvents)
                    propagateEvents[geneEvent.gene] = geneEvent.propagateEvent;

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

                    if (!extension.skillChanges.NullOrEmpty() && gene.geneClass == typeof(Gene_SkillChanging))
                        foreach (SkillChange change in extension.skillChanges)
                            if (change.skillChange != IntRange.Zero)
                            {
                                skillChanging.Add(gene);
                                break;
                            }

                    if (extension.downedMemory != null || extension.downedByMechMemory != null || 
                        extension.downedByPawnMemory != null || extension.downedByInsectMemory != null ||
                        extension.downedByHumanlikeMemory != null || extension.downedByEntityMemory != null || 
                        extension.downedByAnimalMemory != null)
                        downedMemoryGenes.Add(gene);

                    if (extension.propagateEvent != null)
                        propagateEvents[gene] = extension.propagateEvent;
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

                PostLovinThingsExtension lovinExtension = gene.GetModExtension<PostLovinThingsExtension>();
                if (lovinExtension != null)
                {
                    lovinAddinGenes.Add(gene);
                    if (lovinExtension.partnerMemory != null)
                        partnerLovinMemoryReplacer.Add(gene);
                }

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
            nameColorHediffs = new List<HediffDef>();

            foreach (HediffDef hediff in DefDatabase<HediffDef>.AllDefs)
            {
                if (hediff.comps.NullOrEmpty()) continue;
                foreach (HediffCompProperties comp in hediff.comps)
                {
                    if (comp is HediffCompProperties_ExplodingAttacks || comp is HediffCompProperties_ExplodingRangedAttacks || comp is HediffCompProperties_ExplodingMeleeAttacks)
                        explosiveAttackHediffs.Add(hediff);
                    else if (comp is HediffCompProperties_TemporarySkillChange skillChange)
                    {
                        if (!skillChange.skillChanges.Where((arg) => arg.skillChange != IntRange.Zero).EnumerableNullOrEmpty())
                            skillChangeHediffs.Add(hediff);
                    }
                    else if (comp is HediffCompProperties_Shield)
                        shieldHediffs.Add(hediff);
                    else if (comp is HediffCompProperties_NameColor)
                        nameColorHediffs.Add(hediff);
                }
            }
        }

        private void CacheThingsOfInterest()
        {
            shieldEquipment = new List<ThingDef>();
            meleeWeaponTraits = new List<WeaponTraitDef>();
            projectileOverrideTraits = new List<WeaponTraitDef>();

            foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs)
            {
                needEquippableAbilityPatches |= thing.HasComp<CompAbilityLimitedCharges>();
                
                if (thing.HasComp<CompShieldEquipment>())
                    shieldEquipment.Add(thing);
            }

            if (ModsConfig.RoyaltyActive || ModsConfig.OdysseyActive)
                foreach (WeaponTraitDef trait in DefDatabase<WeaponTraitDef>.AllDefs)
                {
                    WeaponTraitExtension extension = trait.GetModExtension<WeaponTraitExtension>();
                    if (extension != null)
                    {
                        if (!extension.extraMeleeDamages.NullOrEmpty() || extension.meleeDamageDefOverride != null)
                            meleeWeaponTraits.Add(trait);
                        if (extension.projectileOverride != null)
                            projectileOverrideTraits.Add(trait);
                    }
                }
        }
    }
}

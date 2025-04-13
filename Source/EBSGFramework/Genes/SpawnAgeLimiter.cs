using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    // Named as such because this was originally solely for spawn age limiting, it now serves as a universal base for various things
    public class SpawnAgeLimiter : Gene
    {
        public int cachedGeneCount = 0;

        public int rerenderInterval = 500;

        public int lastChangeTick = -1;

        public int stage = 0;

        public List<AbilityDef> addedAbilities;

        public EBSGExtension cachedExtension;

        public EBSGExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    cachedExtension = def.GetModExtension<EBSGExtension>();
                }
                return cachedExtension;
            }
        }

        public override bool Active
        {
            get
            {
                if (!base.Active) return false;

                if (Extension != null)
                {
                    // Make it active for the remove past age to force the removal on tick
                    if (Extension.removePastAge > 0 && pawn.ageTracker.AgeBiologicalYearsFloat > Extension.removePastAge)
                        return true;
                    if (Extension.maxAgeActive > 0 && pawn.ageTracker.AgeBiologicalYearsFloat > Extension.maxAgeActive)
                        return false;
                }

                return true;
            }
        }

        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            base.Notify_IngestedThing(thing, numTaken);
            if (Extension?.hediffsToGivePostConsumption.NullOrEmpty() == false)
                foreach (HediffToParts hediffSet in Extension.hediffsToGivePostConsumption)
                    if (!hediffSet.consumedThings.NullOrEmpty() && hediffSet.consumedThings.Contains(thing.def))
                        pawn.AddHediffToParts(null, hediffSet);
        }

        public override void PostAdd()
        {
            if (!Active || Overridden) return;
            base.PostAdd();
            if (Extension != null)
            {
                GetGender(pawn, Extension, def);
                if (!Extension.geneAbilities.NullOrEmpty()) addedAbilities = AbilitiesWithCertainGenes(pawn, Extension.geneAbilities, addedAbilities);
                LimitAge(pawn, Extension.expectedAges, Extension.ageRange, Extension.sameBioAndChrono);
                if (!Extension.mutationGeneSets.NullOrEmpty()) pawn.GainRandomGeneSet(Extension.inheritable, Extension.removeGenesFromOtherLists, Extension.mutationGeneSets);
                pawn.AddHediffToParts(Extension.hediffsToApplyAtAges);
            }
            if (def.HasModExtension<EquipRestrictExtension>() && (pawn.equipment != null || pawn.apparel != null))
            {
                EquipRestrictExtension equipRestrict = def.GetModExtension<EquipRestrictExtension>();

                if (equipRestrict.noEquipment)
                {
                    if (pawn.equipment != null)
                        if (pawn.Position.IsValid)
                            pawn.equipment.DropAllEquipment(pawn.Position);
                        else // In theory this should never occur, but it's the final backup for if a pawn happens to somehow gain a gene without having a position
                            pawn.equipment.DestroyAllEquipment();
                    pawn.apparel?.DropAllOrMoveAllToInventory();
                }
                else
                {
                    if (!equipRestrict.forbiddenEquipments.NullOrEmpty())
                    {
                        if (pawn.apparel != null && !pawn.apparel.WornApparel.NullOrEmpty())
                        {
                            List<Apparel> apparels = new List<Apparel>(pawn.apparel.WornApparel);
                            foreach (Apparel apparel in apparels)
                                if (equipRestrict.forbiddenEquipments.Contains(apparel.def))
                                    pawn.apparel.TryDrop(apparel);
                        }
                        if (pawn.equipment != null && !pawn.equipment.AllEquipmentListForReading.NullOrEmpty())
                        {
                            List<ThingWithComps> equipment = new List<ThingWithComps>(pawn.equipment.AllEquipmentListForReading);
                            foreach (ThingWithComps thing in equipment)
                                if (equipRestrict.forbiddenEquipments.Contains(thing.def))
                                    if (pawn.Position.IsValid)
                                        pawn.equipment.TryDropEquipment(thing, out var droppedEquip, pawn.Position);
                                    else
                                        pawn.equipment.DestroyEquipment(thing);
                        }
                    }


                    if (pawn.apparel != null)
                        if (equipRestrict.noApparel)
                            pawn.apparel.DropAllOrMoveAllToInventory();
                        else if ((!equipRestrict.limitedToApparels.NullOrEmpty() || !equipRestrict.limitedToEquipments.NullOrEmpty()) && !pawn.apparel.WornApparel.NullOrEmpty())
                        {
                            List<Apparel> apparels = new List<Apparel>(pawn.apparel.WornApparel);
                            foreach (Apparel apparel in apparels)
                                if (!CheckEquipLists(equipRestrict.limitedToApparels, equipRestrict.limitedToEquipments, apparel.def))
                                    pawn.apparel.TryDrop(apparel);
                        }

                    if (pawn.equipment != null)
                        if (equipRestrict.noWeapons)
                            if (pawn.Position.IsValid)
                                pawn.equipment.DropAllEquipment(pawn.Position);
                            else
                                pawn.equipment.DestroyAllEquipment();
                        else if ((!equipRestrict.limitedToWeapons.NullOrEmpty() || !equipRestrict.limitedToEquipments.NullOrEmpty()) && !pawn.equipment.AllEquipmentListForReading.NullOrEmpty())
                        {
                            List<ThingWithComps> equipment = new List<ThingWithComps>(pawn.equipment.AllEquipmentListForReading);
                            foreach (ThingWithComps thing in equipment)
                                if (!CheckEquipLists(equipRestrict.limitedToWeapons, equipRestrict.limitedToEquipments, thing.def))
                                    if (pawn.Position.IsValid)
                                        pawn.equipment.TryDropEquipment(thing, out var droppedEquip, pawn.Position);
                                    else
                                        pawn.equipment.DestroyEquipment(thing);
                        }
                }
            }

            EBSGBodyExtension bodyExtension = def.GetModExtension<EBSGBodyExtension>();
            if (bodyExtension != null)
            {
                pawn.Drawer.renderer.SetAllGraphicsDirty();
                rerenderInterval = bodyExtension.interval;
            }
            else
                rerenderInterval = 0;
        }

        // Things represents the temporary list, while equipment represents the universal one. thing is the item in question. False means it wasn't in either list
        public bool CheckEquipLists(List<ThingDef> things, List<ThingDef> equipment, ThingDef thing)
        {
            if (!things.NullOrEmpty() && things.Contains(thing)) return true;
            if (!equipment.NullOrEmpty() && equipment.Contains(thing)) return true;
            return false;
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(300))
            {
                if (Extension?.geneAbilities.NullOrEmpty() == false && pawn.genes != null && pawn.genes.GenesListForReading.Count != cachedGeneCount)
                {
                    if (addedAbilities == null) addedAbilities = new List<AbilityDef>();
                    addedAbilities = AbilitiesWithCertainGenes(pawn, Extension.geneAbilities, addedAbilities);
                    cachedGeneCount = pawn.genes.GenesListForReading.Count;
                }
            }

            if (rerenderInterval > 0 && pawn.IsHashIntervalTick(rerenderInterval))
            {
                if (def.HasModExtension<EBSGBodyExtension>()) // Refreshes the drawer to ensure the visual is up to date
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                else
                    rerenderInterval = 0;
            }

            if (pawn.IsHashIntervalTick(2500))
            {
                if (Extension != null)
                {
                    if (Extension.removePastAge > 0 && pawn.ageTracker.AgeBiologicalYearsFloat > Extension.removePastAge)
                    {
                        pawn.genes.RemoveGene(this);
                        return;
                    }
                    if (!Extension.genderByAge.NullOrEmpty() && (Extension.genderByAge.Count > 1 || Extension.genderByAge[0].range != GenderByAge.defaultRange))
                        GetGender(pawn, Extension, def);
                    pawn.AddHediffToParts(Extension.hediffsToApplyAtAges, null, true);
                }
            }
        }

        public override void PostRemove()
        {
            if (Extension != null && !Extension.hediffsToApplyAtAges.NullOrEmpty())
                pawn.RemoveHediffsFromParts(Extension.hediffsToApplyAtAges);
        }

        public static void GetGender(Pawn pawn, EBSGExtension extension, GeneDef def)
        {
            if (extension?.genderByAge.NullOrEmpty() == false)
                foreach (GenderByAge genderByAge in extension.genderByAge)
                    if (genderByAge.range.ValidValue(pawn.ageTracker.AgeBiologicalYearsFloat))
                    { 
                        if (genderByAge.gender != Gender.None)
                        {
                            if (!genderByAge.gender.Equals(pawn.gender)) return;
                            pawn.gender = genderByAge.gender;
                            switch (pawn.gender)
                            {
                                case Gender.Female:
                                    if (def.bodyType == null && pawn.story?.bodyType == BodyTypeDefOf.Male)
                                    {
                                        pawn.story.bodyType = BodyTypeDefOf.Female;
                                        pawn.Drawer.renderer.SetAllGraphicsDirty();
                                    }
                                    break;
                                case Gender.Male:
                                    if (def.bodyType == null && pawn.story?.bodyType == BodyTypeDefOf.Female)
                                    {
                                        pawn.story.bodyType = BodyTypeDefOf.Male;
                                        pawn.Drawer.renderer.SetAllGraphicsDirty();
                                    }
                                    pawn.RemovePregnancies();
                                    break;
                            }
                        }
                        return;
                    }
        }

        public static void LimitAge(Pawn pawn, FloatRange expectedAges, FloatRange ageRange, bool sameBioAndChrono = false, bool removeChronic = true)
        {
            if (ageRange.max > 0)
            {
                float currentBioAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                float currentChronoAge = pawn.ageTracker.AgeChronologicalYearsFloat;
                if (!expectedAges.Includes(currentBioAge))
                {
                    currentBioAge = ageRange.RandomInRange;
                    pawn.ageTracker.AgeBiologicalTicks = (long)(currentBioAge * 3600000f);
                }
                if (sameBioAndChrono && currentBioAge != currentChronoAge)
                {
                    pawn.ageTracker.AgeChronologicalTicks = (long)(currentBioAge * 3600000f);
                }
                if (removeChronic) pawn.RemoveChronicHediffs();
            }
        }

        public static List<AbilityDef> AbilitiesWithCertainGenes(Pawn pawn, List<AbilityAndGeneLink> geneAbilities, List<AbilityDef> addedAbilities)
        {
            List<AbilityDef> abilitiesToAdd = new List<AbilityDef>();

            pawn.RemovePawnAbilities(addedAbilities);

            foreach (AbilityAndGeneLink link in geneAbilities)
            {
                if (link.abilities.NullOrEmpty()) continue;
                if (pawn.CheckGeneTrio(link.requireOneOfGenes, link.requiredGenes, link.forbiddenGenes))
                    foreach (AbilityDef ability in link.abilities) abilitiesToAdd.Add(ability);
            }

            return pawn.GivePawnAbilities(abilitiesToAdd);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastChangeTick, "lastChangeTick", -1);
            Scribe_Values.Look(ref stage, "stage", 0);
            Scribe_Collections.Look(ref addedAbilities, "EBSG_AddedAbilities");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EBSGBodyExtension bodyExtension = def.GetModExtension<EBSGBodyExtension>();
                if (bodyExtension != null)
                    rerenderInterval = bodyExtension.interval;
                else
                    rerenderInterval = 0;
            }
        }
    }
}

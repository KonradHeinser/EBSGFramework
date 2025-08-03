using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace EBSGFramework
{
    public class CompAbilityEffect_CreateItems : CompAbilityEffect
    {
        public new CompProperties_AbilityCreateItems Props => (CompProperties_AbilityCreateItems)props;

        public Dictionary<IntVec3, ThingPatternPart> partList = new Dictionary<IntVec3, ThingPatternPart>();

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = parent.pawn.Map;
            List<Thing> list = new List<Thing>();
            list.AddRange(AffectedCells(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
                                                                               where t.def.category == ThingCategory.Item && (t.def != partList[c].thing || t.def.stackLimit == 1)
                                                                               select t));
            foreach (Thing item in list)
            {
                item.DeSpawn();
            }
            foreach (IntVec3 item2 in AffectedCells(target, map))
            {
                if (partList[item2].thing == null) continue;
                Thing thing = ThingMaker.MakeThing(partList[item2].thing);
                thing.stackCount = Math.Min(partList[item2].count, partList[item2].thing.stackLimit);

                CompSpawnBaby babyComp = thing.TryGetComp<CompSpawnBaby>();
                if (babyComp != null)
                {
                    Pawn mother = null;
                    Pawn father = null;

                    if (Props.linkingHediff != null && parent.pawn.HasHediff(Props.linkingHediff))
                    {
                        parent.pawn.health.hediffSet.TryGetHediff(Props.linkingHediff, out Hediff hediff);
                        if (hediff is HediffWithTarget linkingHediff && linkingHediff.target is Pawn partner)
                            if (partner.gender == Gender.Male)
                            {
                                mother = parent.pawn;
                                father = partner;
                            }
                            else
                            {
                                mother = partner;
                                father = parent.pawn;
                            }
                    }
                    else
                    {
                        if (parent.pawn.gender == Gender.Male) father = parent.pawn;
                        else mother = parent.pawn;
                    }

                    babyComp.mother = mother;
                    babyComp.father = father;
                    babyComp.faction = parent.pawn.Faction;
                }

                GenSpawn.Spawn(thing, item2, map);
                if (Props.sendSkipSignal) CompAbilityEffect_Teleport.SendSkipUsedSignal(item2, parent.pawn);
            }
            foreach (Thing item3 in list)
            {
                IntVec3 intVec = IntVec3.Invalid;
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 intVec2 = item3.Position + GenRadial.RadialPattern[i];
                    if (intVec2.InBounds(map) && intVec2.Walkable(map) && map.thingGrid.ThingsListAtFast(intVec2).Count <= 0)
                    {
                        intVec = intVec2;
                        break;
                    }
                }
                if (intVec != IntVec3.Invalid)
                {
                    GenSpawn.Spawn(item3, intVec, map);
                }
                else
                {
                    GenPlace.TryPlaceThing(item3, item3.Position, map, ThingPlaceMode.Near);
                }
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(AffectedCells(target, parent.pawn.Map).ToList(), Valid(target) ? Color.white : Color.red);
        }

        private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
        {
            if (!Props.thingPattern.NullOrEmpty())
            {
                partList.Clear();
                foreach (ThingPatternPart patternPart in Props.thingPattern)
                {
                    if ((patternPart.thing != null && patternPart.count > 0) || patternPart.reservedForLargeThing)
                    {
                        IntVec3 intVec = target.Cell + new IntVec3(patternPart.relativeLocation.x, 0, patternPart.relativeLocation.z);
                        if ((!patternPart.skipIfBlocked || (!intVec.Filled(parent.pawn.Map) && intVec.Standable(parent.pawn.Map))) && !partList.ContainsKey(intVec))
                        {
                            partList.Add(intVec, patternPart);
                            yield return intVec;
                        }
                    }
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages)) return false;

            Map map = parent.pawn.Map;
            if (AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.Filled(map)))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => !c.Standable(map)))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityUnwalkable".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.pollutedForbidden && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.IsPolluted(map)))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityPollution".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.pollutedRequired && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => !c.IsPolluted(parent.pawn.Map)))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNoPollution".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.waterForbidden && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.GetTerrain(parent.pawn.Map).IsWater))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityWater".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.waterRequired && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => !c.GetTerrain(parent.pawn.Map).IsWater))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNoWater".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.roofForbidden && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.Roofed(parent.pawn.Map)))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityRoofed".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.roofForbidden && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => !c.Roofed(parent.pawn.Map)))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNoRoof".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.roofForbidden && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.GetPlant(parent.pawn.Map) != null))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNoRoof".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.noPlants && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.GetPlant(parent.pawn.Map) != null))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityPlant".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (Props.noPushing)
            {
                List<Thing> list = new List<Thing>();
                list.AddRange(AffectedCells(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
                                                                                   where t.def.category == ThingCategory.Item && (t.def != partList[c].thing || t.def.stackLimit == 1)
                                                                                   select t));
                if (!list.NullOrEmpty())
                {
                    if (throwMessages)
                    {
                        Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityThing".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                    }
                    return false;
                }
            }
            if (Props.noBuildings && AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.GetFirstBuilding(map) != null))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return true;
        }
    }
}

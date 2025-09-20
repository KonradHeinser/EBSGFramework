using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class CompAbilityEffect_DestroyCorpse : CompAbilityEffect
    {
        public new CompProperties_AbilityDestroyCorpse Props => (CompProperties_AbilityDestroyCorpse)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Thing?.Destroyed != false && target.Thing is Corpse corpse)
            {
                IntVec3 position = corpse.PositionHeld;
                Map map = corpse.MapHeld;
                Pawn victim = corpse.InnerPawn;
                float bodySizeMult = victim != null ? victim.BodySize : 1f;
                float bloodLossMult = corpse.RemainingBlood();

                if (Props.makeFilth && Props.bloodFilthToSpawnRange != IntRange.Zero)
                {
                    float bloodMutliplier = (Props.multiplyBloodByBodySize ? bodySizeMult : 1f) * 
                        (Props.multiplyBloodByRemainingBlood ? bloodLossMult : 1f);

                    int randomInRange = (int)(Props.bloodFilthToSpawnRange.RandomInRange * bloodMutliplier);
                    for (int i = 0; i < randomInRange; i++)
                    {
                        IntVec3 c = position;
                        if (randomInRange > 1)
                            c = c.RandomAdjacentCell8Way();

                        if (randomInRange > 10)
                        {
                            float radiusChecker = 10;
                            while (randomInRange > radiusChecker)
                            {
                                c = c.RandomAdjacentCell8Way();
                                radiusChecker *= 2f;
                            }
                        }
                        if (c.InBounds(map))
                        {
                            ThingDef bloodType = Props.filthReplacement != null && Props.filthReplacement.thingClass == typeof(Filth) ?
                                Props.filthReplacement : victim?.RaceProps.BloodDef ?? ThingDefOf.Filth_Blood;

                            FilthMaker.TryMakeFilth(c, map, bloodType, victim?.LabelShort);
                        }
                    }
                }

                if (Props.thingToMake != null)
                {
                    ThingDef stuff = Props.stuff;
                    if (stuff == null && !Props.thingToMake.stuffCategories.NullOrEmpty())
                        if (victim != null && Props.thingToMake.stuffCategories.Contains(StuffCategoryDefOf.Leathery))
                            stuff = victim.RaceProps.leatherDef;
                        else stuff = Props.thingToMake.defaultStuff;
                    Thing thing = ThingMaker.MakeThing(Props.thingToMake, stuff);
                    thing.stackCount = Props.count > 0 ? Props.count : Mathf.CeilToInt(bodySizeMult * Props.bodySizeFactor);
                    GenSpawn.Spawn(thing, position, map);
                }

                Props.explosionSound?.PlayOneShot(new TargetInfo(position, map));

                if (Props.hemogenGain != 0)
                {
                    var hemogen = parent.pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
                    if (hemogen != null)
                    {
                        var hGain = Props.hemogenGain * 
                            (Props.multiplyHemogenByBodySize ? bodySizeMult : 1f) * 
                            (Props.multiplyHemogenByRemainingBlood ? bloodLossMult : 1f);
                        GeneUtility.OffsetHemogen(parent.pawn, hGain);
                    }
                }

                if (Props.resourceGain != 0)
                {
                    var r = parent.pawn.genes?.GetGene(Props.resourceMainGene);
                    if (r is ResourceGene resource)
                    {
                        var rGain = Props.resourceGain *
                            (Props.multiplyResourceByBodySize ? bodySizeMult : 1f) *
                            (Props.multiplyResourceByRemainingBlood ? bloodLossMult: 1f);
                        ResourceGene.OffsetResource(parent.pawn, rGain, resource, null, Props.useResourceGainFactor);
                    }
                }

                if (Props.nutritionGain != 0 && parent.pawn.needs.food != null)
                {
                    var nGain = Props.nutritionGain *
                        (Props.multiplyNutritionByBodySize ? bodySizeMult : 1f) *
                        (Props.multiplyNutritionByRemainingBlood ? bloodLossMult : 1f);
                    parent.pawn.needs.food.CurLevel += nGain;
                }

                corpse.Destroy();
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (Props.mustBeHemogenic && parent.pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() == null)
            {
                reason = "AbilityDisabledNoHemogenGene".Translate(parent.pawn);
                return true;
            }
            if (Props.mustHaveResourceGene && !parent.pawn.HasRelatedGene(Props.resourceMainGene))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.resourceMainGene.LabelCap);
                return true;
            }
            return base.GizmoDisabled(out reason);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) && Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Thing?.Destroyed != false)
                return false;
            if (!(target.Thing is Corpse))
                return false;
            return base.Valid(target, throwMessages);
        }
    }
}

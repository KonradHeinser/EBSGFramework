using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class CompAbilityEffect_InstantKill : CompAbilityEffect
    {
        public new CompProperties_InstantKill Props => (CompProperties_InstantKill)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing != null && target.Thing is Pawn victim && !victim.Dead)
            {
                IntVec3 position = victim.PositionHeld;
                Map map = victim.MapHeld;
                float bodySizeMult = victim.BodySize;

                if (Props.makeFilth && Props.bloodFilthToSpawnRange != IntRange.Zero)
                {
                    float bloodMutliplier = Props.multiplyBloodByBodySize ? bodySizeMult : 1f;

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

                            FilthMaker.TryMakeFilth(c, map, bloodType, victim.LabelShort);
                        }
                    }
                }

                if (Props.thingToMake != null)
                {
                    ThingDef stuff = Props.stuff;
                    if (stuff == null && !Props.thingToMake.stuffCategories.NullOrEmpty())
                        if (Props.thingToMake.stuffCategories.Contains(StuffCategoryDefOf.Leathery))
                            stuff = victim.RaceProps.leatherDef;
                        else stuff = Props.thingToMake.defaultStuff;
                    Thing thing = ThingMaker.MakeThing(Props.thingToMake, stuff);
                    thing.stackCount = Props.count > 0 ? Props.count : Mathf.CeilToInt(bodySizeMult * Props.bodySizeFactor);
                    GenSpawn.Spawn(thing, position, map);
                }

                Props.explosionSound?.PlayOneShot(new TargetInfo(position, map));

                DamageDef damageToReport = Props.damageDefToReport ??
                    (ModsConfig.BiotechActive ? DamageDefOf.Vaporize : DamageDefOf.Burn);

                victim.TakeDamage(new DamageInfo(damageToReport, 99999f, 999f, -1f, parent.pawn, victim.health.hediffSet.GetBrain()));

                if (Props.deleteCorpse) victim?.Corpse?.Destroy(DestroyMode.KillFinalize);
            }
        }
    }
}

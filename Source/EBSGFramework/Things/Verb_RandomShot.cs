using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EBSGFramework
{
    public class Verb_RandomShot : Verb_Shoot
    {
        public RandomShotExtension cachedExtension;

        public RandomShotExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    cachedExtension = HediffSource?.def.GetModExtension<RandomShotExtension>() ??
                        EquipmentSource?.def.GetModExtension<RandomShotExtension>();
                }

                return cachedExtension;
            }
        }

        public override ThingDef Projectile
        {
            get
            {
                return Extension?.projectiles?.RandomElementByWeight((arg) => arg.weight)?.projectile ?? base.Projectile;
            }
        }
    }

    public class RandomShotExtension : DefModExtension
    {
        public List<RandomProjectilePackage> projectiles = new List<RandomProjectilePackage>();
    }

    public class RandomProjectilePackage
    {
        public ThingDef projectile;
        public int weight = 1;
    }
}

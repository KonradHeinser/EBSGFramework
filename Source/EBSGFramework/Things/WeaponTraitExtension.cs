using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class WeaponTraitExtension : DefModExtension
    {
        public DamageDef meleeDamageDefOverride;

        public List<ExtraDamage> extraMeleeDamages;

        public ThingDef projectileOverride;
    }
}

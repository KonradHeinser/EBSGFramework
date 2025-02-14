using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class PostLovinThingsExtension : DefModExtension
    {
        public List<GeneDef> partnerRequiresOneOf; // If the partner doesn't have one of these genes, then this stuff doesn't occur

        public List<GeneDef> partnerHasNoneOf;

        public ThoughtDef selfMemory;

        public ThoughtDef partnerMemory;

        public Gender gender = Gender.None;

        public Gender partnerGender = Gender.None;

        public List<ThingDefCountClass> spawnThings;

        public List<HediffToParts> hediffsToApplySelf;

        public List<HediffToParts> hediffsToApplyPartner;

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public DamageDef damageToSelf;

        public int damageToSelfAmount = -1;

        public List<BodyPartDef> selfBodyParts;

        public float selfDamageChance = 1f;

        public DamageDef damageToPartner;

        public int damageAmount = -1;

        public List<BodyPartDef> partnerBodyParts;

        public float partnerDamageChance = 1f;
    }
}

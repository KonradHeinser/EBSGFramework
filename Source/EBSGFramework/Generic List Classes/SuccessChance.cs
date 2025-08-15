using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class SuccessChance
    {
        public float baseSuccessChance = 1f;

        public StatDef casterStatChance;

        public StatEffect casterStatEffect = StatEffect.Multiply;

        public StatDef targetStatChance;

        public StatEffect targetStatEffect = StatEffect.Divide;

        public string successMessage = null;

        public string failureMessage = null;

        public float Chance(Pawn caster, Thing target)
        {
            float chance = baseSuccessChance;
            if (caster != null && casterStatChance != null)
            {
                float val = caster.StatOrOne(casterStatChance);
                switch (casterStatEffect)
                {
                    case StatEffect.Divide:
                        if (val != 0)
                            chance /= val;
                        break;
                    case StatEffect.Multiply:
                        chance *= val;
                        break;
                    case StatEffect.OneMinusDivide:
                        if (val != 1)
                            chance /= (1 - val);
                        break;
                    case StatEffect.OneMinusMultiply:
                        chance *= (1 - val);
                        break;
                }
            }
            if (target != null && targetStatChance != null)
            {
                float val = target.StatOrOne(targetStatChance);
                switch (targetStatEffect)
                {
                    case StatEffect.Divide:
                        if (val != 0)
                            chance /= val;
                        break;
                    case StatEffect.Multiply:
                        chance *= val;
                        break;
                    case StatEffect.OneMinusDivide:
                        if (val != 1)
                            chance /= (1 - val);
                        break;
                    case StatEffect.OneMinusMultiply:
                        chance *= (1 - val);
                        break;
                }
            }
            return Mathf.Clamp01(chance);
        }

        public bool Success(Pawn caster, Thing target)
        {
            if (!Rand.Chance(Chance(caster, target)))
            {
                if (failureMessage != null && caster.Faction.IsPlayer)
                    Messages.Message(failureMessage.TranslateOrFormat(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            if (successMessage != null && caster.Faction.IsPlayer)
                Messages.Message(successMessage.TranslateOrFormat(), MessageTypeDefOf.NeutralEvent);
            return true;
        }
    }
}

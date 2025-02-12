using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
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
            Log.Message($"Chance A: {chance}");
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
            Log.Message($"Chance B: {chance}");
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
            Log.Message($"Chance C: {chance}");
            return Mathf.Clamp01(chance);
        }

        public bool Success(Pawn caster, Thing target)
        {
            if (!Rand.Chance(Chance(caster, target)))
            {
                if (failureMessage != null && caster.Faction.IsPlayer)
                    Messages.Message(failureMessage.TranslateOrLiteral(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            if (successMessage != null && caster.Faction.IsPlayer)
                Messages.Message(successMessage.TranslateOrLiteral(), MessageTypeDefOf.NeutralEvent);
            return true;
        }
    }
}

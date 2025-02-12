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

        public bool casterStatDivides = false;

        public StatDef targetStatChance;

        public bool targetStatMultiplies = false;

        public string successMessage = null;

        public string failureMessage = null;

        public float Chance(Pawn caster, Thing target)
        {
            float chance = baseSuccessChance;

            if (caster != null && casterStatChance != null)
            {
                float val = caster.StatOrOne(casterStatChance);
                if (!casterStatDivides || val == 0) chance *= val;
                else chance /= val;
            }

            if (target != null && targetStatChance != null)
            {
                float val = target.StatOrOne(targetStatChance);
                if (!targetStatMultiplies && val != 0) chance /= val; 
                else chance *= val;
            }

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

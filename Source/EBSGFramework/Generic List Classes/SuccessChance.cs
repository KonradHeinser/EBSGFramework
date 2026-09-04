using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class SuccessChance
    {
        public float baseSuccessChance = 1f;

        public StatDef casterStatChance;

        public Effect casterStatEffect = Effect.Multiply;
        
        public List<SkillDef> casterSkills;
        
        public SimpleCurve casterSkillCurve;
        
        public Effect casterSkillEffect = Effect.Multiply;

        public StatDef targetStatChance;

        public Effect targetStatEffect = Effect.Divide;
        
        public List<SkillDef> targetSkills;
        
        public SimpleCurve targetSkillCurve;
        
        public Effect targetSkillEffect = Effect.Multiply;

        public string successMessage = null;

        public string failureMessage = null;
        
        public bool hideChance = false;

        public string ExtraLabelMouseAttachment(Pawn caster, LocalTargetInfo target)
        {
            if (hideChance || !target.HasThing)
                return null;
            
            return "EBSG_SuccessChance".Translate(Math.Round(Chance(caster, target.Thing == caster ? null : target.Thing) * 100, 3));
        }

        public float Chance(Pawn caster, Thing target)
        {
            float chance = baseSuccessChance;
            if (caster != null)
            {
                if (casterStatChance != null)
                    Affect(casterStatEffect, ref chance, caster.StatOrOne(casterStatChance));
                if (!casterSkills.NullOrEmpty() && casterSkillCurve != null && caster.skills != null)
                    Affect(casterSkillEffect, ref chance, casterSkillCurve.Evaluate(casterSkills.Sum(s => caster.skills.GetSkill(s).Level)));
            }

            if (target != null)
            {
                if (targetStatChance != null)
                    Affect(targetStatEffect, ref chance, target.StatOrOne(targetStatChance));
                if (!targetSkills.NullOrEmpty() && targetSkillCurve != null && target is Pawn p && p.skills != null)
                    Affect(targetSkillEffect, ref chance, targetSkillCurve.Evaluate(targetSkills.Sum(s => p.skills.GetSkill(s).Level)));
            }
            return Mathf.Clamp01(chance);
        }

        public static void Affect(Effect effect, ref float chance, float val)
        {
            switch (effect)
            {
                case Effect.Divide:
                    if (val != 0)
                        chance /= val;
                    break;
                case Effect.Multiply:
                    chance *= val;
                    break;
                case Effect.OneMinusDivide:
                    if (val != 1)
                        chance /= (1 - val);
                    break;
                case Effect.OneMinusMultiply:
                    chance *= (1 - val);
                    break;
                case Effect.Subtract:
                    chance -= val;
                    break;
                case Effect.Add:
                    chance += val;
                    break;
            }
        }

        public bool Success(Pawn caster, Thing target)
        {
            if (!Rand.Chance(Chance(caster, target)))
            {
                if (failureMessage != null && caster.Faction.IsPlayer)
                    Messages.Message(failureMessage.TranslateOrFormat(caster.LabelShortCap, target.LabelShortCap), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            if (successMessage != null && caster.Faction.IsPlayer)
                Messages.Message(successMessage.TranslateOrFormat(caster.LabelShortCap, target.LabelShortCap), MessageTypeDefOf.NeutralEvent);
            return true;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_GiveHediffUntilLimit : CompAbilityEffect
    {
        public new CompProperties_AbilityGiveHediffUntilLimit Props => props as CompProperties_AbilityGiveHediffUntilLimit;

        public Pawn Caster => parent.pawn;

        public float Max => Props.maxSeverity == 0 ? Props.hediff.maxSeverity : Props.maxSeverity;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Props.affectTarget && target.Pawn != null)
            {
                var hediffs = target.Pawn.GetHediffFromParts(Props.hediff, new List<BodyPartDef>{Props.part});
                var severity = Props.severity.RandomInRange;
                if (Props.factorStat != null)
                    severity *= target.Pawn.StatOrOne(Props.factorStat);
                if (Props.divisorStat != null)
                {
                    var divisor = target.Pawn.StatOrOne(Props.divisorStat);
                    severity /= divisor == 0 ? 0.0001f : divisor;
                }
                severity = Mathf.Clamp(severity, 0, Max);
                
                if (hediffs.Any())
                {
                    var h = hediffs.First();
                    if (h.Severity < Max) 
                        h.Severity = Mathf.Clamp(h.Severity + severity, 0, Max);
                }
                else
                    target.Pawn.AddHediffToPart(target.Pawn.health.hediffSet.GetBodyPartRecord(Props.part), Props.hediff, severity, severity, false, Caster);
            }
            
            if (Props.affectSelf)
            {
                var hediffs = Caster.GetHediffFromParts(Props.hediff, new List<BodyPartDef>{Props.part});
                var severity = Props.severity.RandomInRange;
                if (Props.factorStat != null)
                    severity *= Caster.StatOrOne(Props.factorStat);
                if (Props.divisorStat != null)
                {
                    var divisor = Caster.StatOrOne(Props.divisorStat);
                    severity /= divisor == 0 ? 0.0001f : divisor;
                }
                severity = Mathf.Clamp(severity, 0, Max);
                
                if (hediffs.Any())
                {
                    var h = hediffs.First();
                    if (h.Severity < Max) 
                        h.Severity = Mathf.Clamp(h.Severity + severity, 0, Max);
                }
                else
                    Caster.AddHediffToPart(Caster.health.hediffSet.GetBodyPartRecord(Props.part), Props.hediff, severity, severity, false, target.Pawn);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (Props.affectTarget && !Props.affectSelf && target.Pawn == null)
                return false;

            var t = Props.affectTarget;
            var s = Props.affectSelf;

            if (t)
            {
                var hediffs = target.Pawn?.GetHediffFromParts(Props.hediff, new List<BodyPartDef> { Props.part });
                if (hediffs?.Any() == true && hediffs.First().Severity >= Max)
                    t = false;
            }
            
            if (s)
            {
                var hediffs = Caster.GetHediffFromParts(Props.hediff, new List<BodyPartDef> { Props.part });
                if (hediffs.Any() && hediffs.First().Severity >= Max)
                    s = false;
            }

            if (!t && !s)
                return false;

            return base.Valid(target, throwMessages);
        }
    }
}

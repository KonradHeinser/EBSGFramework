using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffUntilLimit
    {
        public HediffDef hediff;

        public FloatRange severity = FloatRange.One;

        public StatDef factorStat;

        public StatDef divisorStat;
        
        public FloatRange severityRangeLimits = new FloatRange(0, float.MaxValue);
        
        public float maxSeverity = 0;
        
        public BodyPartDef part;

        public bool affectTarget = true;
        
        public bool affectSelf = false;

        public float Max => maxSeverity == 0 ? hediff.maxSeverity : maxSeverity;
        
        public float Severity(Pawn pawn)
        {
            var s = severity.RandomInRange;
            if (factorStat != null)
                s *= pawn.StatOrOne(factorStat);
            if (divisorStat != null)
            {
                var divisor = pawn.StatOrOne(divisorStat);
                s /= divisor == 0 ? 0.0001f : divisor;
            }
            s = Mathf.Clamp(s, 0, Max);
            s = severityRangeLimits.ClampToRange(s);
            return s;
        }

        public bool CanAffect(Pawn pawn)
        {
            if (pawn == null) return false;
            var hediffs = pawn?.GetHediffFromParts(hediff, new List<BodyPartDef> { part });
            if (hediffs.NullOrEmpty()) return true;
            return hediffs?.First().Severity + Severity(pawn) < Max;
        }

        public void AffectPawns(Pawn caster, Pawn target)
        {
            if (affectSelf && caster != null)
            {
                var hediffs = caster.GetHediffFromParts(hediff, new List<BodyPartDef> { part });
                var s = Severity(caster);
                if (hediffs.Any())
                {
                    var h = hediffs.First();
                    if (h.Severity < Max) 
                        h.Severity = Mathf.Clamp(h.Severity + s, 0, Max);
                }
                else
                    caster.AddHediffToPart(caster.health.hediffSet.GetBodyPartRecord(part), hediff, s, s, false, target);
            }

            if (affectTarget && target != null)
            {
                var hediffs = target.GetHediffFromParts(hediff, new List<BodyPartDef> { part });
                var s = Severity(target);
                if (hediffs.Any())
                {
                    var h = hediffs.First();
                    if (h.Severity < Max) 
                        h.Severity = Mathf.Clamp(h.Severity + s, 0, Max);
                }
                else
                    target.AddHediffToPart(target.health.hediffSet.GetBodyPartRecord(part), hediff, s, s, false, caster);
            }
        }
    }
}
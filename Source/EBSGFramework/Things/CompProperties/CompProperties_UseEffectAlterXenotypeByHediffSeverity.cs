using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_UseEffectAlterXenotypeByHediffSeverity : CompProperties_UseEffect
    {
        public HediffDef hediff;

        public float severityChange = 1;

        public List<ComplexXenoAlter> xenotypes; // Need to make a new item so genes can be manually removed

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public bool sendMessage = true;

        public Prerequisites prerequisites;

        public CompProperties_UseEffectAlterXenotypeByHediffSeverity()
        {
            compClass = typeof(CompUseEffect_AlterXenotypeByHediffSeverity);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var i in base.ConfigErrors(parentDef))
                yield return i;

            if (hediff == null)
                yield return "hediff cannot be null";

            if (xenotypes.NullOrEmpty())
                yield return "need to have at least one xenotype available";
        }
    }
}

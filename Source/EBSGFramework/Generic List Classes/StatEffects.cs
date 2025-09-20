using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class StatEffects
    {
        public StatDef parentFactor;

        public StatRequirement parentFactorReq = StatRequirement.Always;

        public StatDef parentDivisor;

        public StatRequirement parentDivisorReq = StatRequirement.Higher;

        public StatDef otherFactor;

        public StatRequirement otherFactorReq = StatRequirement.Always;

        public StatDef otherDivisor;

        public StatRequirement otherDivisorReq = StatRequirement.Higher;

        public float FinalFactor(Thing parent, Thing other)
        {
            var num = 1f;

            if (parent != null) // Not sure how it'd be null, but weirder things have been witnessed
            {
                if (parentFactor != null)
                    num *= parent.StatOrOne(parentFactor, parentFactorReq);

                if (parentDivisor != null)
                    num /= parent.StatOrOne(parentDivisor, parentDivisorReq);
            }

            if (other != null)
            {
                if (otherFactor != null)
                    num *= other.StatOrOne(otherFactor, otherFactorReq);

                if (otherDivisor != null)
                    num /= other.StatOrOne(otherDivisor, otherDivisorReq);
            }

            return num;
        }
    }
}

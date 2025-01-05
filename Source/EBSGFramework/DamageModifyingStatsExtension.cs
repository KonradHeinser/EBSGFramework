using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class DamageModifyingStatsExtension : DefModExtension
    {
        public List<StatDef> outgoingAttackerFactors;

        public StatRequirement outAttackFactorReq = StatRequirement.Always;

        public List<StatModifier> outgoingAttackerModifiers;

        public StatRequirement outAttackModReq = StatRequirement.Always;

        public List<StatDef> outgoingAttackerDivisors;

        public StatRequirement outAttackDivReq = StatRequirement.Always;

        public List<StatDef> outgoingTargetFactors;

        public StatRequirement outTargetFactorReq = StatRequirement.Always;

        public List<StatModifier> outgoingTargetModifiers;

        public StatRequirement outTargetModReq = StatRequirement.Always;

        public List<StatDef> outgoingTargetDivisors;

        public StatRequirement outTargetDivReq = StatRequirement.Always;

        public float maxOutRemaining = -1f;

        public float minOutRemaining = 0f;

        public bool Outgoing
        {
            get
            {
                return !outgoingAttackerFactors.NullOrEmpty() ||
                    !outgoingAttackerModifiers.NullOrEmpty() ||
                    !outgoingAttackerDivisors.NullOrEmpty() ||
                    !outgoingTargetFactors.NullOrEmpty() ||
                    !outgoingTargetModifiers.NullOrEmpty() ||
                    !outgoingTargetDivisors.NullOrEmpty() ||
                    minOutRemaining > 0 || maxOutRemaining != -1f;
            }
        }

        public List<StatDef> incomingAttackerFactors;

        public StatRequirement inAttackFactorReq = StatRequirement.Always;

        public List<StatModifier> incomingAttackerModifiers;

        public StatRequirement inAttackModReq = StatRequirement.Always;

        public List<StatDef> incomingAttackerDivisors;

        public StatRequirement inAttackDivReq = StatRequirement.Always;

        public List<StatDef> incomingTargetFactors;

        public StatRequirement inTargetFactorReq = StatRequirement.Always;

        public List<StatModifier> incomingTargetModifiers;

        public StatRequirement inTargetModReq = StatRequirement.Always;

        public List<StatDef> incomingTargetDivisors;

        public StatRequirement inTargetDivReq = StatRequirement.Always;

        public float maxInRemaining = -1f;

        public float minInRemaining = 0f;

        public bool Incoming
        {
            get
            {
                return !incomingAttackerFactors.NullOrEmpty() ||
                    !incomingAttackerModifiers.NullOrEmpty() ||
                    !incomingAttackerDivisors.NullOrEmpty() ||
                    !incomingTargetFactors.NullOrEmpty() ||
                    !incomingTargetModifiers.NullOrEmpty() ||
                    !incomingTargetDivisors.NullOrEmpty() ||
                    minInRemaining > 0 || maxInRemaining != -1f;
            }
        }
    }
}

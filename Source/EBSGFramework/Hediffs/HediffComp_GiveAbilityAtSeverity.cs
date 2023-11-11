using System;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_GiveAbilityAtSeverity : HediffComp
    {
        private HediffCompProperties_GiveAbilityAtSeverity Props => (HediffCompProperties_GiveAbilityAtSeverity)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            CheckAbilities();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!base.Pawn.IsHashIntervalTick(60))
            {
                return;
            }
            CheckAbilities();
        }

        private void CheckAbilities()
        {
            foreach (AbilitiesAtSeverities severitySet in Props.abilitiesAtSeverities)
            {
                if (parent.Severity >= severitySet.minSeverity && parent.Severity <= severitySet.maxSeverity)
                {
                    if (severitySet.abilityDef != null)
                    {
                        parent.pawn.abilities.GainAbility(severitySet.abilityDef);
                    }
                    if (!severitySet.abilityDefs.NullOrEmpty())
                    {
                        for (int i = 0; i < severitySet.abilityDefs.Count; i++)
                        {
                            parent.pawn.abilities.GainAbility(severitySet.abilityDefs[i]);
                        }
                    }
                }
                else
                {
                    foreach (Ability ability in parent.pawn.abilities.AllAbilitiesForReading)
                    {
                        if (severitySet.abilityDef != null)
                        {
                            if (ability.def == severitySet.abilityDef)
                            {
                                parent.pawn.abilities.RemoveAbility(severitySet.abilityDef);
                            }
                        }
                        if (!severitySet.abilityDefs.NullOrEmpty())
                        {
                            if (severitySet.abilityDefs.Contains(ability.def))
                            {
                                parent.pawn.abilities.RemoveAbility(ability.def);
                            }
                        }
                    }
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            foreach (AbilitiesAtSeverities severitySet in Props.abilitiesAtSeverities)
            {
                if (severitySet.abilityDef != null)
                {
                    parent.pawn.abilities.RemoveAbility(severitySet.abilityDef);
                }
                if (!severitySet.abilityDefs.NullOrEmpty())
                {
                    for (int i = 0; i < severitySet.abilityDefs.Count; i++)
                    {
                        parent.pawn.abilities.RemoveAbility(severitySet.abilityDefs[i]);
                    }
                }
            }
        }
    }
}

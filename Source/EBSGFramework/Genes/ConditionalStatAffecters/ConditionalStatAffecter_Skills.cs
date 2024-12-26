﻿using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Skills : ConditionalStatAffecter
    {
        public List<SkillCheck> skillLimiters;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_CorrectSkills".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null)
                return req.Pawn.AllSkillLevelsMet(skillLimiters);

            return false;
        }
    }
}
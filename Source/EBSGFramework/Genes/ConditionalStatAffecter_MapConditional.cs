using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_MapConditional : ConditionalStatAffecter
    {
        public List<GameConditionDef> conditions;

        public bool forbiddenConditions = false;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label;
            return "EBSG_MapCondition".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.Pawn != null && req.Pawn.Spawned && req.Pawn.Map.GameConditionManager != null)
            {
                GameConditionManager manager = req.Pawn.Map.GameConditionManager;
                foreach (GameConditionDef condition in conditions)
                    if (manager.ConditionIsActive(condition)) return !forbiddenConditions;

                if (forbiddenConditions) return true;
            }
            return defaultActive;
        }
    }
}

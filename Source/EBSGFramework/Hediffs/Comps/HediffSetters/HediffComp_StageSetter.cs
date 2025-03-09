using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_StageSetter : HediffComp
    {
        HediffCompProperties_StageSetter Props => (HediffCompProperties_StageSetter)props;

        public Command_Action action;

        private Texture2D icon;

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (action == null)
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                if (Props.iconPath != null)
                    icon = ContentFinder<Texture2D>.Get(Props.iconPath);
                else
                    icon = null;
                var stages = parent.def.stages;
                for (var i = 0; i < stages.Count; i++)
                {
                    var stage = stages[i];
                    StageSet set = Props.sets.Count < i ? Props.sets[i] : null;

                    if (stage == parent.CurStage)
                    {
                        if (set?.iconPath != null)
                            icon = ContentFinder<Texture2D>.Get(set.iconPath);
                        continue;
                    }

                    if (set?.prerequisites?.ValidPawn(Pawn) == false)
                        continue;

                    var label = set?.label ?? ((stage.overrideLabel ?? stage.label) ?? parent.Label);

                    options.Add(new FloatMenuOption(label, delegate ()
                    {
                        parent.Severity = stage.minSeverity;
                        action = null;
                    }));
                }

                if (options.NullOrEmpty())
                    options.Add(new FloatMenuOption("EBSG_NoOptionsAvailable".Translate(), null));

                action = new Command_Action
                {
                    defaultLabel = Props.label?.TranslateOrLiteral() ?? parent.LabelCap,
                    defaultDesc = Props.description?.TranslateOrLiteral() ?? parent.Description,
                    icon = icon,
                    action = delegate ()
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }
            yield return action;
        }
    }
}

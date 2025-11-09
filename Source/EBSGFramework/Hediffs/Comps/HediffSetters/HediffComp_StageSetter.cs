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

        private bool? prereqs;

        private bool Prereqs
        {
            get
            {
                if (prereqs == null)
                {
                    if (Props.sets.NullOrEmpty())
                        prereqs = false;
                    else
                        foreach (var set in Props.sets)
                            if (set.prerequisites != null)
                            {
                                prereqs = true;
                                break;
                            }
                }
                return (bool)prereqs;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            // Refreshes regularly to ensure nothing with unmet prereqs remains
            if (action == null || (Prereqs && Pawn.IsHashIntervalTick(30)))
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
                    StageSet set = !Props.sets.NullOrEmpty() && Props.sets.Count > i ? Props.sets[i] : null;

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
                        action = null;

                        if (set != null)
                        {
                            if (set.prerequisites?.ValidPawn(Pawn) == false)
                                return;
                            
                            if (set.ticks > 0)
                                Pawn.stances.SetStance(new Stance_StageSetting(set.ticks, null, null)
                                {
                                    neverAimWeapon = true,
                                });
                        }
                        parent.Severity = stage.minSeverity;
                    }));
                }
                
                if (options.NullOrEmpty())
                    options.Add(new FloatMenuOption("EBSG_NoOptionsAvailable".Translate(), null));
                
                action = new Command_Action
                {
                    defaultLabel = Props.label?.TranslateOrFormat() ?? parent.LabelCap,
                    defaultDesc = Props.description?.TranslateOrFormat() ?? parent.Description,
                    icon = icon,
                    action = delegate ()
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
            }

            if (!Props.usableDuringCooldowns && Pawn.stances.curStance is Stance_Cooldown)
            {
                action.Disabled = true;
                action.disabledReason = "CooldownTime".Translate();
            }
            else
                action.Disabled = false;
            
            yield return action;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class HediffComp_Modular : HediffComp
    {
        public HediffCompProperties_Modular Props => props as HediffCompProperties_Modular;

        public HediffStage cachedInput;
        public HediffStage cachedOutput;
        public bool resetCache = false; //Set to true after changing modules to force a recache
        public bool ejectableModules = true;
        public Command_Action ejectAction;
        public static readonly Texture2D cachedEjectTex = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectModule");

        public ThingOwner<ThingWithComps> moduleHolder;

        public List<ModuleSlot> GetOpenSlots(CompUseEffect_HediffModule comp)
        {
            List<ModuleSlot> validSlots = new List<ModuleSlot>();

            if (!Props.slots.NullOrEmpty())
                foreach (ModuleSlot slot in Props.slots)
                {
                    if (!comp.Props.slotIDs.Contains(slot.slotID))
                        continue;
                    
                    float spaceTaken = 0;
                    bool invalid = false;

                    if (moduleHolder.Count > 0)
                        foreach (ThingWithComps thing in moduleHolder)
                        {
                            CompUseEffect_HediffModule moduleComp = thing.TryGetComp<CompUseEffect_HediffModule>();

                            if (moduleComp.usedSlot == slot.slotID)
                            {
                                spaceTaken += moduleComp.Props.requiredCapacity;

                                if (spaceTaken + comp.Props.requiredCapacity > slot.capacity && slot.capacity != -1)
                                {
                                    invalid = true;
                                    break;
                                }
                            }

                            if (moduleComp.Props.excludeIDs.Intersect(comp.Props.excludeIDs).Count() > 0)
                            {
                                invalid = true;
                                break;
                            }
                        }
                    if (!invalid)
                        validSlots.Add(slot);
                }
            return validSlots;
        }

        public void InstallModule(ThingWithComps thing)
        {
            CompUseEffect_HediffModule comp = thing.TryGetComp<CompUseEffect_HediffModule>();
            comp.Install(this);
            thing.DeSpawnOrDeselect();
            moduleHolder.TryAdd(thing, false);
            resetCache = true;
            RecacheGizmo();
        }

        public void RemoveModule(ThingWithComps thing, bool forced = false)
        {
            CompUseEffect_HediffModule comp = thing.TryGetComp<CompUseEffect_HediffModule>();

            if (comp?.Remove(this) != true)
            {
                if (forced)
                    moduleHolder.Remove(thing);
                return;
            }
            
            moduleHolder.TryDrop(thing, Pawn.Position, Pawn.Map, ThingPlaceMode.Near, 1, out Thing placedThing);
            resetCache = true;
            RecacheGizmo();
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            moduleHolder = new ThingOwner<ThingWithComps>();
            RecacheGizmo();
        }

        public HediffStage GetStage(HediffStage stage)
        {
            if (stage == cachedInput && !resetCache)
                return cachedOutput;
            
            cachedInput = stage;
            resetCache = false;

            if (moduleHolder.Count == 0)
            {
                cachedOutput = stage;
                return cachedOutput;
            }

            cachedOutput = new HediffStage();
            cachedOutput.CopyStageValues(stage);
            
            if (moduleHolder.Count > 0)
                foreach (ThingWithComps module in moduleHolder)
                    cachedOutput = module.TryGetComp<CompUseEffect_HediffModule>().
                        ModifyStage(parent.CurStageIndex, cachedOutput);
            return cachedOutput;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref moduleHolder, "moduleHolder");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                if (moduleHolder == null)
                    moduleHolder = new ThingOwner<ThingWithComps>();
                else if (moduleHolder.Count > 0)
                    foreach (ThingWithComps thing in moduleHolder)
                    {
                        CompUseEffect_HediffModule comp = thing.TryGetComp<CompUseEffect_HediffModule>();
                        comp?.GenerateComps(this);
                    }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Props.dropModulesOnRemoval)
                while (moduleHolder.Count > 0)
                    RemoveModule(moduleHolder[0], true);
        }

        public void RecacheGizmo()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            ejectableModules = true;
            int validOptions = 0;

            if (moduleHolder.Count > 0)
                foreach (ThingWithComps module in moduleHolder)
                {
                    CompUseEffect_HediffModule comp = module.TryGetComp<CompUseEffect_HediffModule>();

                    if (!comp.Props.ejectable)
                    {
                        options.Add(new FloatMenuOption($"{"EBSG_CannotEject".Translate()} {module.LabelCap} ({comp.GetSlot.slotName.TranslateOrLiteral()})", null));
                        continue;
                    }

                    options.Add(new FloatMenuOption($"{"EBSG_Eject".Translate()} {module.LabelCap} ({comp.GetSlot.slotName.TranslateOrLiteral()})", delegate () { RemoveModule(module); }));
                    validOptions += 1;
                }

            if (options.Count == 0 || validOptions == 0)
            {
                options.Add(new FloatMenuOption("No ejectable modules", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                ejectableModules = false;
            }

            ejectAction = new Command_Action();
            ejectAction.defaultLabel = ($"{"EBSG_EjectModuleFrom".Translate()} {parent.LabelCap}");
            ejectAction.defaultDesc = ($"{"EBSG_EjectModuleFrom".Translate()} {parent.LabelCap} {(parent.Part != null ? $" ({parent.Part.LabelCap})" : "")}");
            ejectAction.icon = cachedEjectTex;
            ejectAction.action = delegate ()
            {
                Find.WindowStack.Add(new FloatMenu(options));
            };
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (ejectAction == null)
                RecacheGizmo();

            if (!ejectableModules)
                yield break;

            yield return ejectAction;
            yield break;
        }
    }
}

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
        public bool resetCache; //Set to true after changing modules to force a recache
        public bool ejectableModules = true;
        public Command_Action ejectAction;
        public static readonly Texture2D cachedEjectTex = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectModule");

        public ThingOwner<ThingWithComps> moduleHolder;

        public List<ModuleSlot> GetOpenSlots(CompUseEffect_HediffModule comp)
        {
            return GetOpenSlots(comp.Props);
        }
        
        public List<ModuleSlot> GetOpenSlots(CompProperties_UseEffectHediffModule comp)
        {
            var validSlots = new List<ModuleSlot>();

            if (!Props.slots.NullOrEmpty())
                foreach (var slot in Props.slots)
                {
                    if (!comp.slotIDs.Contains(slot.slotID))
                        continue;
                    
                    float spaceTaken = 0;
                    var invalid = false;

                    if (moduleHolder.Count > 0)
                        foreach (var thing in moduleHolder)
                        {
                            var moduleComp = thing.TryGetComp<CompUseEffect_HediffModule>();

                            if (moduleComp.usedSlot == slot.slotID)
                            {
                                spaceTaken += moduleComp.Props.requiredCapacity;

                                if (spaceTaken + comp.requiredCapacity > slot.capacity && slot.capacity != -1)
                                {
                                    invalid = true;
                                    break;
                                }
                            }

                            if (!moduleComp.Props.excludeIDs.NullOrEmpty() && !comp.excludeIDs.NullOrEmpty() &&
                                !moduleComp.Props.excludeIDs.Intersect(comp.excludeIDs).EnumerableNullOrEmpty())
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
            var comp = thing.TryGetComp<CompUseEffect_HediffModule>();
            comp.Install(this);
            thing.DeSpawnOrDeselect();
            moduleHolder.TryAdd(thing, false);
            resetCache = true;
            RecacheGizmo();
        }
        
        public void RemoveModule(ThingWithComps thing, bool forced = false)
        {
            var comp = thing.TryGetComp<CompUseEffect_HediffModule>();

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
                foreach (var module in moduleHolder)
                    cachedOutput = module.TryGetComp<CompUseEffect_HediffModule>().ModifyStage(parent.CurStageIndex, cachedOutput);
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
                    foreach (var thing in moduleHolder)
                    {
                        var comp = thing.TryGetComp<CompUseEffect_HediffModule>();
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

        private void RecacheGizmo()
        {
            if (!Pawn.Spawned) // Avoids world gen issues
                return;

            var options = new List<FloatMenuOption>();
            ejectableModules = true;
            var validOptions = 0;
            
            if (moduleHolder.Count > 0)
                foreach (var module in moduleHolder)
                {
                    var comp = module.TryGetComp<CompUseEffect_HediffModule>();

                    if (!comp.Props.ejectable)
                    {
                        options.Add(new FloatMenuOption($"{"EBSG_CannotEject".Translate()} {module.LabelCap} ({comp.GetSlot.slotName.TranslateOrFormat()})", null));
                        continue;
                    }
                    options.Add(new FloatMenuOption($"{"EBSG_Eject".Translate()} {module.LabelCap} ({comp.GetSlot.slotName.TranslateOrFormat()})", delegate () { RemoveModule(module); }));
                    validOptions += 1;
                }

            if (options.Count == 0 || validOptions == 0)
            {
                options.Add(new FloatMenuOption("No ejectable modules", null));
                ejectableModules = false;
            }

            ejectAction = new Command_Action
            {
                defaultLabel = ($"{"EBSG_EjectModuleFrom".Translate()} {parent.LabelCap}"),
                defaultDesc = ($"{"EBSG_EjectModuleFrom".Translate()} {parent.LabelCap} {(parent.Part != null ? $" ({parent.Part.LabelCap})" : "")}"),
                icon = cachedEjectTex,
                action = delegate ()
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            // If the pawn isn't spawned but getting comps for any reason, we don't want them trying to remove stuff
            if (!Pawn.Spawned) 
                yield break;

            if (ejectAction == null)
                RecacheGizmo();

            if (!ejectableModules)
                yield break;

            yield return ejectAction;
        }
    }
}

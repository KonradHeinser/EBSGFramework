using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    internal class HediffComp_Renderable : HediffComp, IRenderable, IColorSelector
    {
        private HediffCompProperties_Renderable Props => props as HediffCompProperties_Renderable;

        public Mote attachedMote;
        public Effecter attachedEffecter;

        public static readonly Texture2D cachedPaletteTex = ContentFinder<Texture2D>.Get("UI/Gizmos/ColorPalette");
        public Color? primaryColor;
        public Color? secondaryColor;
        public bool? usePrimary;
        public bool? useSecondary;
        public Command_Action paletteAction;
        public List<HediffGraphicPackage> additionalGraphics;

        public virtual Color PrimaryColor
        {
            get
            {
                if (primaryColor != null)
                {
                    return primaryColor.Value;
                }

                if ((Props.primaryGenerator & ColorPackageGenerator.Favorite) != 0)
                {
                    if (Pawn.story != null && Pawn.story.favoriteColor != null)
                    {
                        primaryColor = Pawn.story.favoriteColor;
                        return primaryColor.Value;
                    }
                }

                if ((Props.primaryGenerator & ColorPackageGenerator.Ideology) != 0)
                {
                    if (Pawn.Ideo != null)
                    {
                        primaryColor = Pawn.Ideo.Color;
                        return primaryColor.Value;
                    }
                }

                if ((Props.primaryGenerator & ColorPackageGenerator.Faction) != 0)
                {
                    if (Pawn.Faction != null)
                    {
                        primaryColor = Pawn.Faction.Color;
                        return primaryColor.Value;
                    }
                }

                if ((Props.primaryGenerator & ColorPackageGenerator.Random) != 0)
                {
                    primaryColor = new Color(Rand.Value, Rand.Value, Rand.Value);
                    return primaryColor.Value;
                }

                if ((Props.primaryGenerator & ColorPackageGenerator.White) != 0)
                {
                    primaryColor = Color.white;
                    return primaryColor.Value;
                }

                primaryColor = Props.defaultPrimary;

                return primaryColor.Value;
            }

            set
            {
                primaryColor = value;
            }
        }

        public virtual Color SecondaryColor
        {
            get
            {
                if (secondaryColor != null)
                {
                    return secondaryColor.Value;
                }

                if ((Props.secondaryGenerator & ColorPackageGenerator.Favorite) != 0)
                {
                    if (Pawn.story != null && Pawn.story.favoriteColor != null)
                    {
                        secondaryColor = Pawn.story.favoriteColor;
                        return secondaryColor.Value;
                    }
                }

                if ((Props.secondaryGenerator & ColorPackageGenerator.Ideology) != 0)
                {
                    if (Pawn.Ideo != null)
                    {
                        secondaryColor = Pawn.Ideo.Color;
                        return secondaryColor.Value;
                    }
                }

                if ((Props.secondaryGenerator & ColorPackageGenerator.Faction) != 0)
                {
                    if (Pawn.Faction != null)
                    {
                        secondaryColor = Pawn.Faction.Color;
                        return secondaryColor.Value;
                    }
                }

                if ((Props.secondaryGenerator & ColorPackageGenerator.Random) != 0)
                {
                    secondaryColor = new Color(Rand.Value, Rand.Value, Rand.Value);
                    return secondaryColor.Value;
                }

                if ((Props.secondaryGenerator & ColorPackageGenerator.White) != 0)
                {
                    secondaryColor = Color.white;
                    return secondaryColor.Value;
                }

                secondaryColor = Props.defaultPrimary;

                return secondaryColor.Value;
            }

            set
            {
                secondaryColor = value;
            }
        }

        public virtual bool UseSecondary
        {
            get
            {
                return (bool)useSecondary;
            }
        }

        public virtual void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
            if (Props.onlyRenderWhenDrafted && (Pawn.drafter == null || !Pawn.drafter.Drafted))
            {
                return;
            }

            if (Props.graphicData != null)
            {
                Props.graphicData.Graphic.Draw(new Vector3(drawPos.x, Props.altitude.AltitudeFor(), drawPos.z), Pawn.Rotation, Pawn);
            }

            DrawSecondaries(drawPos, bodyType);
        }

        public virtual void DrawSecondaries(Vector3 drawPos, BodyTypeDef bodyType)
        {
            if (Props.additionalGraphics == null)
            {
                return;
            }

            if (additionalGraphics == null)
            {
                RecacheGraphicData();
            }

            for (int i = additionalGraphics.Count - 1; i >= 0; i--)
            {
                HediffGraphicPackage package = additionalGraphics[i];
                Vector3 offset = new Vector3();

                if (!package.CanRender(parent, bodyType, Pawn))
                {
                    continue;
                }

                if (package.offsets != null)
                {
                    if (package.offsets.Count == 4)
                    {
                        offset = package.offsets[Pawn.Rotation.AsInt];
                    }
                    else
                    {
                        offset = package.offsets[0];
                    }
                }

                package.GetGraphic(parent, bodyType).Draw(drawPos + offset, Pawn.Rotation, Pawn);
            }
        }

        public virtual void RecacheGraphicData()
        {
            if (Props.additionalGraphics == null)
            {
                additionalGraphics = new List<HediffGraphicPackage>();
            }
            else
            {
                additionalGraphics = new List<HediffGraphicPackage>(Props.additionalGraphics);
            }

            for (int i = parent.comps.Count - 1; i >= 0; i--)
            {
                IHediffGraphicGiver modular = parent.comps[i] as IHediffGraphicGiver;

                if (modular != null)
                {
                    additionalGraphics = additionalGraphics.Concat(modular.GetAdditionalGraphics).ToList();
                }
            }

            usePrimary = false;
            useSecondary = false;

            for (int i = additionalGraphics.Count - 1; i >= 0; i--)
            {
                HediffGraphicPackage package = additionalGraphics[i];

                if (package.firstMask == HediffPackageColor.PrimaryColor || package.secondMask == HediffPackageColor.PrimaryColor)
                {
                    usePrimary = true;
                }

                if (package.firstMask == HediffPackageColor.SecondaryColor || package.secondMask == HediffPackageColor.SecondaryColor)
                {
                    usePrimary = true;
                    useSecondary = true;
                    break;
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Props.attachedMoteDef != null)
            {
                if (attachedMote == null || attachedMote.Destroyed)
                {
                    attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
                }

                attachedMote.Maintain();
            }

            if (Props.attachedEffecterDef != null)
            {
                if (attachedEffecter == null)
                {
                    attachedEffecter = Props.attachedEffecterDef.SpawnAttached(Pawn, Pawn.Map);
                }

                attachedEffecter.EffectTick(Pawn, Pawn);
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Props.attachedMoteDef != null)
            {
                attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
            }

            if (Props.attachedEffecterDef != null)
            {
                attachedEffecter = Props.attachedEffecterDef.SpawnAttached(Pawn, Pawn.Map);
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            AthenaCache.AddCache(this, ref AthenaCache.renderCache, Pawn.thingIDNumber);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref primaryColor, "primaryColor");
            Scribe_Values.Look(ref secondaryColor, "secondaryColor");

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                AthenaCache.AddCache(this, ref AthenaCache.renderCache, Pawn.thingIDNumber);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            AthenaCache.RemoveCache(this, AthenaCache.renderCache, Pawn.thingIDNumber);

            if (attachedMote != null && !attachedMote.Destroyed)
            {
                attachedMote.Destroy();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (usePrimary == null || useSecondary == null)
            {
                RecacheGraphicData();
            }

            if (!(bool)usePrimary)
            {
                yield break;
            }

            if (paletteAction == null)
            {
                paletteAction = new Command_Action();
                paletteAction.defaultLabel = "Change colors for " + parent.LabelCap;
                paletteAction.icon = cachedPaletteTex;
                paletteAction.action = delegate ()
                {
                    Find.WindowStack.Add(new Dialog_ColorPalette(this));
                };
            }

            yield return paletteAction;
            yield break;
        }
    }

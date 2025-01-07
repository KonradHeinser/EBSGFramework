using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class CompShieldEquipment : ThingComp
    {
        public CompProperties_ShieldEquipment Props => (CompProperties_ShieldEquipment)props;

        public float energy;
        public int ticksToReset = -1;
        public int lastImpactTick = -1;
        public int lastResetTick = -1;
        public Vector3 impactAngleVect;

        public Matrix4x4 matrix;
        public Gizmo_ShieldStatus gizmo;

        public Mote attachedMote;
        public Effecter attachedEffecter;
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        public float MaxEnergy => parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
        public float EnergyRechargeRate => parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

        private Pawn PawnOwner
        {
            get
            {
                if (parent is Apparel apparel)
                    return apparel.Wearer;
                if (parent is Pawn p)
                    return p;
                if (parent.ParentHolder is Pawn_EquipmentTracker tracker)
                    return tracker.pawn;
                return null;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                // Same conditions as the vanilla shield comp

                if (ticksToReset > 0)
                    return false;

                if (PawnOwner == null)
                    return false;

                if (!PawnOwner.Spawned || PawnOwner.Dead || PawnOwner.Downed)
                    return false;

                if (!Props.onlyRenderWhenDrafted || PawnOwner.InAggroMentalState || PawnOwner.Drafted)
                    return true;

                if (PawnOwner.Faction.HostileTo(Faction.OfPlayer) && !PawnOwner.IsPrisoner)
                    return true;

                if (ModsConfig.BiotechActive && PawnOwner.IsColonyMech && Find.Selector.SingleSelectedThing == PawnOwner)
                    return true;

                return false;
            }
        }

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (ShouldDisplay)
                Draw();
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (ShouldDisplay)
                Draw();
        }

        private void Draw()
        {
            Vector3 drawPos = PawnOwner.Drawer.DrawPos;
            float angle = Props.spinning ? Rand.Range(0, 360) : 0;

            if (Props.graphicData != null)
            {
                drawPos.y = Props.altitude.AltitudeFor();
                float scale = Props.minDrawSize + (Props.maxDrawSize - Props.minDrawSize) * energy / MaxEnergy;

                if (Props.scaleWithOwner)
                {
                    if (PawnOwner.RaceProps.Humanlike)
                    {
                        scale *= PawnOwner.DrawSize.x;
                    }
                    else
                    {
                        scale = (scale - 1) + PawnOwner.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.x;
                    }
                }

                if (lastImpactTick > Find.TickManager.TicksGame - 8)
                {
                    float tickScaleModifier = (8 - Find.TickManager.TicksGame + lastImpactTick) / 8f * 0.05f;
                    drawPos += impactAngleVect * tickScaleModifier;
                    scale -= tickScaleModifier;

                    if (lastResetTick > Find.TickManager.TicksGame - 20)
                    {
                        tickScaleModifier = 1 - (20 - Find.TickManager.TicksGame + lastResetTick) / 20f;
                        scale *= tickScaleModifier;
                    }
                }

                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(scale, 1f, scale));
                Graphics.DrawMesh(MeshPool.plane10, matrix, Props.graphicData.Graphic.MatSingle, 0);
            }
            else
            {
                float num = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, energy);
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - lastImpactTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    drawPos += impactAngleVect * num3;
                    num -= num3;
                }
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default;
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Props.attachedMoteDef != null)
            {
                if (attachedMote == null || attachedMote.Destroyed)
                {
                    attachedMote = MoteMaker.MakeAttachedOverlay(PawnOwner, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
                }

                attachedMote.Maintain();
            }

            if (Props.attachedEffecterDef != null)
            {
                if (attachedEffecter == null)
                {
                    attachedEffecter = Props.attachedEffecterDef.SpawnAttached(PawnOwner, PawnOwner.Map);
                }

                attachedEffecter.EffectTick(PawnOwner, PawnOwner);
            }

            if (ticksToReset > 0)
            {
                ticksToReset--;

                if (ticksToReset <= 0)
                {
                    Reset();
                }
            }

            if (PawnOwner == null)
            {
                energy = 0;
                return;
            }

            if (energy >= MaxEnergy)
            {
                return;
            }

            energy = Math.Min(energy + EnergyRechargeRate, MaxEnergy);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            energy = MaxEnergy * Props.energyOnReset;
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }

            if (Props.displayGizmo)
            {
                if (PawnOwner.Faction?.IsPlayer == true && Find.Selector.SingleSelectedThing == PawnOwner)
                {
                    if (gizmo == null)
                    {
                        gizmo = new Gizmo_ShieldStatus(this);
                    }

                    yield return gizmo;
                }
            }

            yield break;
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            if (PawnOwner == null || ticksToReset > 0) return;
            lastImpactTick = Find.TickManager.TicksGame;

            if (!Props.shatterOn.NullOrEmpty() && Props.shatterOn.Contains(dinfo.Def))
            {
                Shatter();
                return;
            }

            if (dinfo.Def.isRanged)
            {
                if (!Props.blocksRangedDamage)
                    return;
            }
            else if (dinfo.Def.isExplosive)
            {
                if (!Props.blocksExplosions)
                    return;
            }
            else
            {
                if (!Props.blocksMeleeDamage)
                    return;
            }

            if (!Props.blockedDamageDefs.NullOrEmpty() && !Props.blockedDamageDefs.Contains(dinfo.Def))
                return;

            if (!Props.ignoredDamageDefs.NullOrEmpty() && Props.ignoredDamageDefs.Contains(dinfo.Def))
                return;

            float epdm = Props.energyPerDamageModifier;

            if (!Props.damageInfoPacks.NullOrEmpty())
                foreach (DamageInfoPack pack in Props.damageInfoPacks)
                {
                    if (dinfo.Def.isRanged)
                    {
                        if (!pack.ranged)
                            continue;
                    }
                    else if (dinfo.Def.isExplosive)
                    {
                        if (!pack.explosions)
                            continue;
                    }
                    else
                    {
                        if (!pack.melee)
                            continue;
                    }

                    if (pack.damageDef != null && pack.damageDef != dinfo.Def)
                        continue;

                    epdm *= pack.factor;
                }

            if (epdm <= 0) return;
            energy -= dinfo.Amount * epdm;

            if (energy <= 0)
            {
                Shatter();

                if (Props.blockOverdamage)
                    absorbed = true;
                else if (Props.reduceDamagePostDestroy)
                    dinfo.SetAmount(-1f * energy / epdm);
            }
            else
            {
                absorbed = true;
                Props.absorbSound?.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map, false));
                impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                Vector3 offsetVector = PawnOwner.DrawPos + impactAngleVect.RotatedBy(180f) * 0.5f;
                float damagePower = Mathf.Min(10f, 2f + dinfo.Amount / 10f);

                if (Props.absorbFleck != null)
                    FleckMaker.Static(offsetVector, PawnOwner.MapHeld, Props.absorbFleck, damagePower);

                for (int i = 0; i < damagePower; i++)
                    FleckMaker.ThrowDustPuff(offsetVector, PawnOwner.MapHeld, Rand.Range(0.8f, 1.2f));
            }
        }

        public virtual void Shatter()
        {
            energy = 0;
            ticksToReset = Props.resetDelay;

            if (Props.shieldBreakEffecter != null)
            {
                float scale = Props.minDrawSize + (Props.maxDrawSize - Props.minDrawSize) * energy / MaxEnergy;
                if (Props.scaleWithOwner)
                    if (PawnOwner.RaceProps.Humanlike)
                        scale *= PawnOwner.DrawSize.x;
                    else
                        scale = (scale - 1) + PawnOwner.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.x;

                Props.shieldBreakEffecter.SpawnAttached(PawnOwner, PawnOwner.MapHeld, scale * 0.5f);
            }

            if (Props.breakFleck != null)
                FleckMaker.Static(PawnOwner.DrawPos, PawnOwner.Map, Props.breakFleck, 12f);

            for (int i = 0; i < 6; i++)
                FleckMaker.ThrowDustPuff(PawnOwner.DrawPos + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), PawnOwner.MapHeld, Rand.Range(0.8f, 1.2f));

            Props.shieldBreakExplosion?.DoExplosion(PawnOwner, PawnOwner.PositionHeld, PawnOwner.MapHeld);
        }

        public void Reset()
        {
            if (PawnOwner?.Spawned == true)
            {
                Props.resetSound?.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.MapHeld, false));
                FleckMaker.ThrowLightningGlow(PawnOwner.DrawPos, PawnOwner.MapHeld, 3f);
            }

            ticksToReset = -1;
            energy = MaxEnergy * Props.energyOnReset;

            lastResetTick = Find.TickManager.TicksGame;
        }

        public override bool CompAllowVerbCast(Verb verb)
        {
            if (Props.blocksRangedWeapons)
                return !(verb is Verb_LaunchProjectile);
            return base.CompAllowVerbCast(verb);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset");
            Scribe_Values.Look(ref lastImpactTick, "lastImpactTick");
            Scribe_Values.Look(ref lastResetTick, "lastResetTick");
            Scribe_Values.Look(ref impactAngleVect, "impactAngleVect");
        }
    }
}

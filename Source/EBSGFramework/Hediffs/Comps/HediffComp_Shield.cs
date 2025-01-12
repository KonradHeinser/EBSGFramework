using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class HediffComp_Shield : HediffComp
    {
        public HediffCompProperties_Shield Props => (HediffCompProperties_Shield)props;
        public float energy;
        public int ticksToReset = -1;
        public int lastImpactTick = -1;
        public int lastResetTick = -1;
        public int lastKeepDisplayTick = -1000;
        public Vector3 impactAngleVect;

        public Matrix4x4 matrix;
        public Gizmo_HediffShieldStatus gizmo;

        public Mote attachedMote;
        public Effecter attachedEffecter;
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        public float MaxEnergy => Mathf.Max((Props.usePawnMaxAndRecharge ? Pawn.GetStatValue(StatDefOf.EnergyShieldEnergyMax) : Props.maxEnergy) * (Props.multiplyMaxBySeverity ? parent.Severity : 1f), 0.01f);
        public float EnergyRechargeRate => (Props.usePawnMaxAndRecharge ? Pawn.GetStatValue(StatDefOf.EnergyShieldRechargeRate) : Props.energyRechargeRate) * (Props.multiplyRechargeBySeverity ? parent.Severity : 1f) / 60f;

        public ShieldState ShieldState
        {
            get
            {
                if (Pawn.IsCharging() || Pawn.IsSelfShutdown())
                {
                    return ShieldState.Disabled;
                }
                CompCanBeDormant comp = Pawn.GetComp<CompCanBeDormant>();
                if (comp != null && !comp.Awake)
                {
                    return ShieldState.Disabled;
                }
                if (ticksToReset <= 0)
                {
                    return ShieldState.Active;
                }
                return ShieldState.Resetting;
            }
        }

        public bool ShouldDisplay
        {
            get
            {
                if (Props.noDisplay) return false;
                // Same conditions as the vanilla shield comp

                if (ShieldState != ShieldState.Active)
                    return false;

                if (!Pawn.Spawned || Pawn.Dead || Pawn.Downed)
                    return false;

                if (!Props.onlyRenderWhenDrafted || Pawn.InAggroMentalState || Pawn.Drafted)
                    return true;

                if (Pawn.Faction?.HostileTo(Faction.OfPlayer) == true && !Pawn.IsPrisoner)
                    return true;

                if (Find.TickManager.TicksGame < lastKeepDisplayTick + 1000)
                    return true;

                if (ModsConfig.BiotechActive && Pawn.IsColonyMech && Find.Selector.SingleSelectedThing == Pawn)
                    return true;

                return false;
            }
        }

        public void ResetDisplayCooldown()
        {
            if (Props.onlyBlockWhileDraftedOrHostile && !Pawn.InAggroMentalState && !Pawn.Drafted &&
                Pawn.LastAttackTargetTick + 1000 < Find.TickManager.TicksGame)
                return;
            lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        public bool Draw(Vector3 drawPos)
        {
            if (!ShouldDisplay) return false;

            float angle = Props.spinning ? Rand.Range(0, 360) : 0;

            if (Props.graphicData != null)
            {
                drawPos.y = Props.altitude.AltitudeFor();
                float scale = Props.minDrawSize + (Props.maxDrawSize - Props.minDrawSize) * energy / MaxEnergy;

                if (Props.scaleWithOwner)
                {
                    if (Pawn.RaceProps.Humanlike)
                    {
                        scale *= Pawn.DrawSize.x;
                    }
                    else
                    {
                        scale = (scale - 1) + Pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.x;
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

            return true;
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

            if (ticksToReset > 0)
            {
                ticksToReset--;

                if (ticksToReset <= 0)
                {
                    Reset();
                }
            }

            if (energy >= MaxEnergy)
            {
                return;
            }

            energy = Math.Min(energy + EnergyRechargeRate, MaxEnergy);
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            energy = MaxEnergy * Props.energyOnReset;
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (!base.CompGetGizmos().EnumerableNullOrEmpty())
                foreach (Gizmo gizmo in base.CompGetGizmos())
                {
                    yield return gizmo;
                }
            
            if (Props.displayGizmo)
            {
                if (Pawn.Faction?.IsPlayer == true && Find.Selector.SingleSelectedThing == Pawn)
                {
                    if (gizmo == null)
                    {
                        gizmo = new Gizmo_HediffShieldStatus(this);
                    }

                    yield return gizmo;
                }
            }
            
            yield break;
        }

        public void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (Props.onlyBlockWhileDraftedOrHostile && !Pawn.InAggroMentalState && !Pawn.Drafted &&
                Pawn.LastAttackTargetTick + 1000 < Find.TickManager.TicksGame)
                return;
            
            if (Pawn == null || ticksToReset > 0) return;
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
                Props.absorbSound?.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
                impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                Vector3 offsetVector = Pawn.DrawPos + impactAngleVect.RotatedBy(180f) * 0.5f;
                float damagePower = Mathf.Min(10f, 2f + dinfo.Amount / 10f);

                if (Props.absorbFleck != null)
                    FleckMaker.Static(offsetVector, Pawn.MapHeld, Props.absorbFleck, damagePower);

                for (int i = 0; i < damagePower; i++)
                    FleckMaker.ThrowDustPuff(offsetVector, Pawn.MapHeld, Rand.Range(0.8f, 1.2f));
            }
        }

        public void Shatter()
        {
            energy = 0;
            ticksToReset = Props.resetDelay;
            Props.shatterSound?.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            if (Props.shieldBreakEffecter != null)
            {
                float scale = Props.minDrawSize + (Props.maxDrawSize - Props.minDrawSize) * energy / MaxEnergy;
                if (Props.scaleWithOwner)
                    if (Pawn.RaceProps.Humanlike)
                        scale *= Pawn.DrawSize.x;
                    else
                        scale = (scale - 1) + Pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.x;

                Props.shieldBreakEffecter.SpawnAttached(Pawn, Pawn.MapHeld, scale * 0.5f);
            }

            if (Props.breakFleck != null)
                FleckMaker.Static(Pawn.DrawPos, Pawn.Map, Props.breakFleck, 12f);

            for (int i = 0; i < 6; i++)
                FleckMaker.ThrowDustPuff(Pawn.DrawPos + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), Pawn.MapHeld, Rand.Range(0.8f, 1.2f));

            Props.shieldBreakExplosion?.DoExplosion(Pawn, Pawn.PositionHeld, Pawn.MapHeld);

            if (Props.removeOnBreak)
                parent.pawn.health.RemoveHediff(parent);
        }

        public void Reset()
        {
            if (Pawn?.Spawned == true)
            {
                Props.resetSound?.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.MapHeld, false));
                FleckMaker.ThrowLightningGlow(Pawn.DrawPos, Pawn.MapHeld, 3f);
            }

            ticksToReset = -1;
            energy = MaxEnergy * Props.energyOnReset;

            lastResetTick = Find.TickManager.TicksGame;
        }

        public bool CompAllowVerbCast(Verb verb)
        {
            if (Props.blocksRangedWeapons)
                return !(verb is Verb_LaunchProjectile);
            return true;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset");
            Scribe_Values.Look(ref lastImpactTick, "lastImpactTick");
            Scribe_Values.Look(ref lastResetTick, "lastResetTick");
            Scribe_Values.Look(ref impactAngleVect, "impactAngleVect");
        }
    }
}

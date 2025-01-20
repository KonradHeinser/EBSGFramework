using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_ShieldEquipment : CompProperties
    {
        // How much energy is lost per unit of damage
        public float energyPerDamageModifier = 0.033f;
        // How long (in ticks) it takes for a shield to go back online after it has been destroyed
        public int resetDelay = 1;
        // What fraction of shield's max energy it has after resetting
        public float energyOnReset = 0.2f;
        // Whenever the shield blocks all damage/stun from the attack that breaks it or not
        public bool blockOverdamage = true;
        // Causes the shield to reduce damage after it's destroyed based on the energy it still had
        public bool reduceDamagePostDestroy = false;

        // Whenever the shield blocks ranged/explosive/melee damage
        public bool blocksRangedDamage = true;
        public bool blocksExplosions = true;
        public bool blocksMeleeDamage = false;
        public bool onlyBlockWhileDraftedOrHostile = false;

        // DamageDefs to check for
        public List<DamageDef> immuneDamageDefs;
        public List<DamageDef> blockedDamageDefs;
        public List<DamageDef> ignoredDamageDefs;

        // List of DamageInfoPack with additional energy cosumption modifiers and overrides for block types for certain DamageDefs
        public List<DamageInfoPack> damageInfoPacks;

        // What types of damage should cause the shield to instantly shatter
        public List<DamageDef> shatterOn;
        public ExplosionData shieldBreakExplosion;

        // Shield sounds and flecks
        public SoundDef absorbSound;
        public SoundDef shatterSound;
        public SoundDef resetSound;

        public FleckDef absorbFleck;
        public FleckDef breakFleck;
        // Effecter that's used upon shield shattering
        public EffecterDef shieldBreakEffecter;

        // Whenever the shield should display a charge gizmo and what text and hover tip should it have
        public bool displayGizmo = true;
        public string gizmoTitle;
        public string gizmoTip;

        // Shield scale based on amount of energy left
        public float minDrawSize = 1.2f;
        public float maxDrawSize = 1.55f;
        // Whenever the shield should scale with owner's draw size
        public bool scaleWithOwner = true;
        // Whenever the shield should have the vanilla spinning effect. Turn off in case you're using custom asymmetric textures
        public bool spinning = true;

        public bool noDisplay = false; // Shield is always invisible
        // Displayed graphic. This graphic is drawn above the pawn at MoteOverhead altitude layer by default
        public GraphicData graphicData;
        // Altitude for graphic rendering
        public AltitudeLayer altitude = AltitudeLayer.MoteOverhead;
        // Mote attached to the equipment piece
        public ThingDef attachedMoteDef;
        // Effecter attached to the equipment piece
        public EffecterDef attachedEffecterDef;
        // Offset of the attached mote
        public Vector3 attachedMoteOffset = new Vector3();
        // Scale of the attached mote
        public float attachedMoteScale = 1f;
        // If graphics should only be rendered when the pawn is drafted
        public bool onlyRenderWhenDrafted = false;

        public bool blocksRangedWeapons = false;

        public CompProperties_ShieldEquipment()
        {
            compClass = typeof(CompShieldEquipment);
        }
    }
}

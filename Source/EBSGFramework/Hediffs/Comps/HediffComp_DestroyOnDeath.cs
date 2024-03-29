﻿using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class HediffComp_DestroyOnDeath : HediffComp
    {
        private HediffCompProperties_DestroyOnDeath Props => (HediffCompProperties_DestroyOnDeath)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (parent.pawn.Corpse == null || parent.pawn.Corpse.Destroyed) return;
            Map map = parent.pawn.Corpse.Map;
            if (map == null) map = parent.pawn.Corpse.MapHeld;

            if (map != null && parent.pawn.Corpse != null)
            {
                if (Props.thingSpawn != null)
                    GenSpawn.Spawn(ThingMaker.MakeThing(Props.thingSpawn), parent.pawn.Corpse.Position, map);
                if (!Props.thingsToSpawn.NullOrEmpty())
                    foreach (ThingDef thing in Props.thingsToSpawn)
                        GenSpawn.Spawn(ThingMaker.MakeThing(thing), parent.pawn.Corpse.Position, map);
                if (Props.extraDeathSound != null) Props.extraDeathSound.PlayOneShot(new TargetInfo(parent.pawn.Corpse.Position, map));
                parent.pawn.Corpse.Destroy();
            }
        }
    }
}

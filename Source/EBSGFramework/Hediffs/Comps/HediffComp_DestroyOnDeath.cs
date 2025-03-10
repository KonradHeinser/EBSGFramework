using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class HediffComp_DestroyOnDeath : HediffComp
    {
        private HediffCompProperties_DestroyOnDeath Props => (HediffCompProperties_DestroyOnDeath)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (Pawn.Corpse == null || Pawn.Corpse.Destroyed) return;
            Map map = Pawn.MapHeld;

            if (map != null && Pawn.Corpse != null)
            {
                if (Props.thingSpawn != null)
                    GenSpawn.Spawn(ThingMaker.MakeThing(Props.thingSpawn), Pawn.PositionHeld, map);
                if (!Props.thingsToSpawn.NullOrEmpty())
                    foreach (ThingDef thing in Props.thingsToSpawn)
                        GenSpawn.Spawn(ThingMaker.MakeThing(thing), Pawn.PositionHeld, map);
                Props.extraDeathSound?.PlayOneShot(new TargetInfo(Pawn.PositionHeld, map));
                Pawn.Corpse.Destroy();
            }
        }
    }
}

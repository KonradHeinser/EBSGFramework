using Verse;

namespace EBSGFramework
{
    public class HediffComp_PreventDeathNotifications : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            Pawn.forceNoDeathNotification = true;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                Pawn.forceNoDeathNotification = true;
        }
    }
}

using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_HemogenOnKill : HediffComp
    {
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);
        }
    }
}

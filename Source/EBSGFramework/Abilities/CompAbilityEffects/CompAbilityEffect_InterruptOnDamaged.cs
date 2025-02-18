using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_InterruptOnDamaged : CompAbilityEffect
    {
        public void Interrupted(Pawn target)
        {
            target?.stances?.stunner?.StopStun();
        }
    }
}

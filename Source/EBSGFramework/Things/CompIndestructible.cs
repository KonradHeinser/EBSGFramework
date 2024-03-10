using Verse;
namespace EBSGFramework
{
    public class CompIndestructible : ThingComp
    {
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            absorbed = true;
        }
    }
}

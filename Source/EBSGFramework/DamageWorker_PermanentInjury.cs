using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class DamageWorker_PermanentInjury : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            dinfo.SetInstantPermanentInjury(true);
            return base.Apply(dinfo, thing);
        }
    }
}

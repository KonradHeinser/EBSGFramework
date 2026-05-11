using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemoveOnFactionChange : HediffComp
    {
        private Faction faction;

        private bool adding; // Flag for knowing if the faction needs to be recorded

        public override bool CompShouldRemove => !adding && faction != Pawn.Faction;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            adding = true;
        }
        
        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (!adding) return;
            // Happens after a brief delay so things like the temporary faction hediff from this mod are certain to be done with their stuff, along with similar effects from other mods
            // Put within interval to make this one check happen marginally less often because registering the faction can wait 25% of a second if needed
            adding = false;
            faction = Pawn.Faction;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref faction, "faction");
        }
    }
}

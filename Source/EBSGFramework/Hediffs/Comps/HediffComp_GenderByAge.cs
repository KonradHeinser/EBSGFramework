using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_GenderByAge : HediffComp
    {
        public HediffCompProperties_GenderByAge Props => props as HediffCompProperties_GenderByAge;

        private Gender original;

        private BeardDef originalBeard = null;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            original = Pawn.gender;
            if (Props.saveBeard)
                originalBeard = Pawn.style.beardDef;
            Pawn.CheckGender(Props.genderByAge, originalBeard);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            // Checking to make sure someone isn't using this as a static switcher
            if (Pawn.IsHashIntervalTick(2500) && (Props.genderByAge.Count() > 1 || Props.genderByAge[0].range != GenderByAge.defaultRange))
                Pawn.CheckGender(Props.genderByAge, originalBeard);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Props.revertPostRemove)
                Pawn.ChangeGender(original, originalBeard);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref original, "original", Gender.None);
            Scribe_Defs.Look(ref originalBeard, "originalBeard");
        }
    }
}

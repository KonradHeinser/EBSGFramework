using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_FlipGenderPeriodically : HediffComp
    {
        public HediffCompProperties_FlipGenderPeriodically Props => props as HediffCompProperties_FlipGenderPeriodically;

        private int interval = 0;

        private Gender original;

        private BeardDef originalBeard = null;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            interval = Props.interval.RandomInRange;
            original = Pawn.gender;
            if (Props.saveBeard)
                originalBeard = Pawn.style.beardDef;
            if (Props.flipPostAdd)
                FlipGender();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            interval -= delta;
            if (interval < 0)
            {
                FlipGender();
                interval += Props.interval.RandomInRange;
            }
        }

        private void FlipGender()
        {
            Gender newGender = Gender.None;
            switch (Pawn.gender)
            {
                case Gender.Male:
                    newGender = Gender.Female;
                    break;
                case Gender.Female:
                    newGender = Gender.Male;
                    break;
            }
            if (newGender != Gender.None) // Checking to make sure the pawn's gender isn't something unexpected
                Pawn.ChangeGender(newGender, originalBeard);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref interval, "interval", 0);
            Scribe_Values.Look(ref original, "original", Pawn.gender);
            Scribe_Defs.Look(ref originalBeard, "originalBeard");
        }
    }
}

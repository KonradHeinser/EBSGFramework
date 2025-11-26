using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByGender : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByGender Props => (HediffCompProperties_SeverityByGender)props;
        
        public Gender oldGender;
        
        protected override bool DoCheck()
        {
            return Pawn.gender != oldGender;
        }

        protected override void SetSeverity()
        {
            foreach (var effect in Props.genders)
                if (Pawn.gender == effect.gender)
                {
                    oldGender = Pawn.gender;
                    parent.Severity = effect.range.RandomInRange;
                }
            ticksToNextCheck = 60000;
        }
    }
}
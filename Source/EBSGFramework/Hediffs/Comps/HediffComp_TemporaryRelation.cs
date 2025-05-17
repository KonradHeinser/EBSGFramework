using Verse;

namespace EBSGFramework
{
    public class HediffComp_TemporaryRelation : HediffComp
    {
        public HediffCompProperties_TemporaryRelation Props => (HediffCompProperties_TemporaryRelation)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (parent is HediffWithTarget p && p.target is Pawn other)
            {
                if (Props.ownRelation != null)
                    Pawn.relations?.AddDirectRelation(Props.ownRelation, other);
                if (Props.otherRelation != null)
                    other.relations?.AddDirectRelation(Props.otherRelation, Pawn);
            }
            else
                Log.Error($"{parent.def} must be a HediffWithTarget or child of that class, and it must target a pawn");
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (parent is HediffWithTarget p && p.target is Pawn other)
            {
                if (Props.ownRelation != null)
                    Pawn.relations?.TryRemoveDirectRelation(Props.ownRelation, other);
                if (Props.otherRelation != null)
                    other.relations?.TryRemoveDirectRelation(Props.otherRelation, Pawn);
            }
        }
    }
}

using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalAbilityCastLastTickOrNever : ThinkNode_Conditional
    {
        public int maxTicksAgo = int.MaxValue;

        public int minTicksAgo;

        public AbilityDef ability;

        protected override bool Satisfied(Pawn pawn)
        {
            Ability ability = pawn.abilities?.GetAbility(this.ability);
            if (ability == null)
                return false;

            if (ability.lastCastTick < 0)
                return true;

            int num = Find.TickManager.TicksGame - ability.lastCastTick;
            if (num > maxTicksAgo)
                return false;

            if (num < minTicksAgo)
                return false;

            return true;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalAbilityCastLastTickOrNever obj = (ThinkNode_ConditionalAbilityCastLastTickOrNever)base.DeepCopy(resolve);
            obj.ability = ability;
            obj.maxTicksAgo = maxTicksAgo;
            obj.minTicksAgo = minTicksAgo;
            return obj;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class GameComponent_EBSGRitualManager : GameComponent
    {
        private Dictionary<RitualDef, int> cooldowns = new Dictionary<RitualDef, int>();

        private List<RitualDef> tmpRitualDefs;

        private List<int> tmpCooldowns;

        public GameComponent_EBSGRitualManager(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (!cooldowns.NullOrEmpty()) {
                var currentTick = Find.TickManager.TicksGame;
                List<RitualDef> removeRituals = new List<RitualDef>();
                foreach (var ritual in cooldowns)
                {
                    if (ritual.Value <= currentTick)
                        removeRituals.Add(ritual.Key);
                }
                if (!removeRituals.NullOrEmpty())
                    foreach (var ritual in removeRituals)
                    {
                        cooldowns.Remove(ritual);
                        if (ritual.sendReadyMessage)
                            if (ritual.readyMessage != null)
                                Messages.Message(ritual.readyMessage.TranslateOrLiteral(ritual.LabelCap), MessageTypeDefOf.PositiveEvent);
                            else
                                Messages.Message("EBSG_RitualReady".Translate(ritual.LabelCap), MessageTypeDefOf.PositiveEvent);
                    }
            }
        }

        public AcceptanceReport Available(RitualDef ritual, Map map)
        {
            var remainingCooldown = GetCooldown(ritual);
            if (remainingCooldown > 0)
                return new AcceptanceReport("EBSG_RitualCooldown".Translate(ritual.LabelCap, remainingCooldown.ToStringTicksToPeriod()).Resolve());
            if (!ritual.allowInPocketMaps && map.IsPocketMap)
                return new AcceptanceReport("EBSG_NoPocketMap".Translate(ritual.LabelCap));
            return true;
        }

        public int GetCooldown(RitualDef ritual)
        {
            if (cooldowns.ContainsKey(ritual))
                return cooldowns[ritual] - Find.TickManager.TicksGame;
            return -1;
        }

        public void StartCooldown(RitualDef ritual)
        {
            cooldowns[ritual] = Find.TickManager.TicksGame + ritual.cooldown;
        }

        public void EndCooldown(RitualDef ritual)
        {
            cooldowns.Remove(ritual);
        }

        public void ClearCooldowns()
        {
            cooldowns.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref cooldowns, "cooldowns", LookMode.Def, LookMode.Value, ref tmpRitualDefs, ref tmpCooldowns);
        }
    }
}

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
    }
}

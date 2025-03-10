using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBSGFramework
{
    public class FlexibleSettings
    {
        public List<Setting> settings;
        public string uniqueID; // Can be the mod's id. Can be the same between your mods, but if another mod has this id, there's a chance that settings will get tied together when they shouldn't be
        public string label; // Section label
    }
}

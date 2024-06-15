using System.Collections.Generic;

namespace EBSGFramework
{
    public class ThinkTreeSetting
    {
        public string uniqueID; // Can be the mod's id. Can be the same between your mods, but if another mod has this id, there's a chance that settings will get tied together when they shouldn't be
        public string label; // Section label
        public string description; // Optional. Seen when hovering over the section label
        public List<ThinkBranchSetting> individualSettings;
    }
}

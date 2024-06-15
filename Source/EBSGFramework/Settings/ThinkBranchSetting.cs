using System;
namespace EBSGFramework
{
    public class ThinkBranchSetting
    {
        public string settingID; // This combined with the uniqueID are what this framework uses to try to keep everything seperate
        public string label; // The label for the setting
        public string description; // Optional and should only be used if your label may cause confusion
        public bool defaultSetting = true; // Defaults to being active, assume the settings before it in the thinktree are active
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBSGFramework
{
    public class Setting
    {
        public SettingType type = SettingType.Toggle;

        public string settingID; // This combined with the uniqueID are what this framework uses to try to keep everything seperate

        public string label; // The label for the setting

        public string description; // Optional and should only be used if your label may cause confusion

        // The default referenced is based on setting type

        public bool defaultToggle = true; // Defaults to being active, assume the settings before it in the thinktree are active

        public float defaultSlide = 0;

        public int defaultIndex = 0;
    }
}

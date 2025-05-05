using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class SettingDef : Def
    {
        public SettingCategoryDef category;
        public SettingType type = SettingType.None;
        public List<string> dropLabels;
        public int defaultValue;
        public bool defaultToggle;
        public FloatRange validRange;
        public float positionInCategory = 0;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
                yield return error;

            if (type == SettingType.None)
                yield return "must have a set setting type.";

            if (category == null)
                yield return "needs a SettingCategoryDef";
        }
    }

    public enum SettingType
    {
        None = 0,
        Toggle = 1,
        Slider = 2,
        SliderInt = 3,
        Dropdown = 4,
        Numeric = 5,
        NumbericInt = 6
    }
}

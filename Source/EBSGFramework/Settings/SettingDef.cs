using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class SettingDef : Def
    {
        public SettingCategoryDef category;
        public SettingType type;
        public int defaultValue;
        public bool defaultToggle;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
                yield return error;

            if (type == 0)
                yield return "must have a set setting type.";

            if (category == null)
                yield return "needs a SettingCategoryDef";
        }
    }

    public enum SettingType
    {
        None,
        Toggle,
        Slider,
        SliderInt,
        Numeric
    }
}

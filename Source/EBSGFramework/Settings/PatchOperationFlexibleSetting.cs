using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class PatchOperationFlexibleSetting : PatchOperationPathed
    {
        public SettingDef setting;

        public FloatRange range = FloatRange.Zero;

        public bool invertRange;

        public FlexAction action = FlexAction.Replace;

        public enum FlexAction
        {
            Replace,
            Factor,
            Offset
        }

        public PatchOperation active;

        public PatchOperation inactive;

        public List<PatchOperation> operations;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (setting.type == SettingType.Toggle)
            {
                if (EBSG_Settings.GetBoolSetting(setting))
                    active?.Apply(xml);
                else
                    inactive?.Apply(xml);
            }
            else if (range != FloatRange.Zero)
            {
                if (range.ValidValue(EBSG_Settings.GetNumSetting(setting)))
                    active?.Apply(xml);
                else
                    inactive?.Apply(xml);
            }
            else if (setting.type == SettingType.Dropdown)
            {
                var num = (int)EBSG_Settings.GetNumSetting(setting);
                if (num < 0)
                    return false;
                if (num < operations.Count && operations[num].GetType() != typeof(PatchOperation))
                    operations[num].Apply(xml);
                else
                    Log.Message(num);
            }
            else
            {
                var num = EBSG_Settings.GetNumSetting(setting);
                var l = xml.SelectNodes(xpath);
                if (l.Count == 0)
                    return false;
                foreach (XmlNode i in l)
                {
                    try
                    {
                        switch (action)
                        {
                            case FlexAction.Replace:
                                i.InnerText = num.ToString();
                                break;
                            case FlexAction.Factor:
                                i.InnerText = (float.Parse(i.InnerText) * num).ToString();
                                break;
                            case FlexAction.Offset:
                                i.InnerText = (float.Parse(i.InnerText) + num).ToString();
                                break;

                        }
                    }
                    catch
                    {
                        Log.Error($"{xpath} did not lead to a number");
                        return false;
                    }
                }
                
            }

            return true;
        }
    }
}

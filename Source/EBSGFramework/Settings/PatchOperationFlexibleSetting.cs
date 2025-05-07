using System.Collections.Generic;
using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class PatchOperationFlexibleSetting : PatchOperationPathed
    {
        public string setting;

        public SettingDef set;

        public FloatRange range = new FloatRange(-9999);

        public float factor = 1f;

        public float offset = 0f;

        public bool invertRange;

        public FlexAction action = FlexAction.Replace;

        public enum FlexAction
        {
            Replace,
            Multiply,
            Divide,
            Offset
        }

        public PatchOperation active;

        public PatchOperation inactive;

        public List<PatchOperation> operations;

        protected override bool ApplyWorker(XmlDocument xml)
        {

            XmlNode node = xml.SelectSingleNode($"Defs/EBSGFramework.SettingDef[defName=\"{setting}\"]");
            if (node == null)
            {
                return false;
            }

            set = DirectXmlToObject.ObjectFromXml<SettingDef>(node, false);
            if (set == null)
            {
                return false;
            }

            if (set.type == SettingType.Toggle)
            {

                if (EBSG_Settings.GetBoolSetting(set))
                    active?.Apply(xml);
                else
                    inactive?.Apply(xml);
            }
            else if (range != new FloatRange(-9999))
            {

                if (range.ValidValue(EBSG_Settings.GetNumSetting(set)) != invertRange)
                    active?.Apply(xml);
                else
                    inactive?.Apply(xml);
            }
            else if (set.type == SettingType.Dropdown)
            {
                var num = (int)EBSG_Settings.GetNumSetting(set);

                if (num < 0)
                    return false;

                if (num < operations.Count && operations[num].GetType() != typeof(PatchOperation))
                    operations[num].Apply(xml);
            }
            else
            {
                var num = EBSG_Settings.GetNumSetting(set);
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
                                i.InnerText = (num + offset * factor).ToString();
                                break;
                            case FlexAction.Multiply:
                                i.InnerText = (float.Parse(i.InnerText) * (num + offset * factor)).ToString();
                                break;
                            case FlexAction.Divide:
                                i.InnerText = (float.Parse(i.InnerText) / (num + offset * factor)).ToString();
                                break;
                            case FlexAction.Offset:
                                i.InnerText = (float.Parse(i.InnerText) + (num + offset * factor )).ToString();
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

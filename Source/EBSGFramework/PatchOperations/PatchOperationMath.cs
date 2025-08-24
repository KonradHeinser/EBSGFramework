using System;
using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class PatchOperationMath : PatchOperationPathed
    {
        public float value;

        public FlexAction action = FlexAction.Multiply;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            var l = xml.SelectNodes(xpath);

            if (l.Count == 0)
                return false;

            foreach (XmlNode i in l)
                try
                {
                    var newValue = value;
                    var oldVal = float.Parse(i.InnerText);
                    switch (action)
                    {
                        case FlexAction.Multiply:
                            newValue *= oldVal;
                            break;
                        case FlexAction.Divide:
                            newValue /= oldVal;
                            break;
                        case FlexAction.Offset:
                            newValue += oldVal;
                            break;
                        case FlexAction.MultRound:
                            newValue *= oldVal;
                            newValue = MathF.Round(newValue);
                            break;
                        case FlexAction.DivRound:
                            newValue /= oldVal;
                            newValue = MathF.Round(newValue);
                            break;
                        default:
                            break;
                    }
                    i.InnerText = (newValue).ToString();
                }
                catch
                {
                    Log.Error($"{xpath} did not lead to a number");
                    return false;
                }

            return true;
        }
    }
}

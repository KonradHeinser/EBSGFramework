using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class PatchOperationMath : PatchOperationPathed
    {
        public float value;
        
        public bool round;

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
                    switch (action)
                    {
                        case FlexAction.Multiply:
                            newValue *= float.Parse(i.InnerText);
                            break;
                        case FlexAction.Divide:
                            newValue /= float.Parse(i.InnerText);
                            break;
                        case FlexAction.Offset:
                            newValue += float.Parse(i.InnerText);
                            break;
                        default:
                            break;
                    }
                    if (round)
                        newValue = MathF.Round(newValue);
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

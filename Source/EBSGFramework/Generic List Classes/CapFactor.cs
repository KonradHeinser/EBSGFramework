using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class CapFactor
    {
        public PawnCapacityDef capacity;

        public float factor = 1;

        public StatDef stat = null;

        public SimpleCurve statCurve = null;

        public bool multiplyFactorBySeverity = false;

        public SimpleCurve severityCurve = null;

        public SimpleCurve curve = null;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "capacity", xmlRoot.Name);

            if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
            {
                if (bool.TryParse(xmlText.InnerText, out bool factor))
                    multiplyFactorBySeverity = factor;
                else
                    DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlText.InnerText);
            }
            else if (node is XmlElement element)
                ParseXmlElement(element);
        }

        private void LoadMultipleNodes(XmlNode xmlRoot)
        {
            foreach (object childNode in xmlRoot.ChildNodes)
                ParseXmlElement(childNode as XmlElement);
        }

        private void ParseXmlElement(XmlElement element)
        {
            if (element.Name == "factor")
                factor = ParseHelper.FromString<float>(element.InnerText);
            else if (element.Name == "statCurve")
                statCurve = DirectXmlToObject.ObjectFromXml<SimpleCurve>(element, true);
            else if (element.Name == "multiplyFactorBySeverity")
                multiplyFactorBySeverity = ParseHelper.FromString<bool>(element.InnerText);
            else if (element.Name == "severityCurve")
                severityCurve = DirectXmlToObject.ObjectFromXml<SimpleCurve>(element, true);
            else if (element.Name == "curve")
                curve = DirectXmlToObject.ObjectFromXml<SimpleCurve>(element, true);
            else
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, element.Name, element.InnerText);
        }
    }
}

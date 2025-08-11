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
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "gene", xmlRoot.Name);

            if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                propagateEvent = ParseHelper.FromString<HistoryEventDef>(xmlText.InnerText);
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
            if (element.Name == "gene")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "gene", element.InnerText);
            else if (element.Name == "propagateEvent")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "propagateEvent", element.InnerText);
        }
    }
}

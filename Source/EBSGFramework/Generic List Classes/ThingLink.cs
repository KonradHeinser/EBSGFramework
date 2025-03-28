﻿using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class ThingLink
    {
        public ThingDef thing;

        public float change;

        public ThingLink()
        {
        }

        public ThingLink(ThingDef thing, float change)
        {
            this.thing = thing;
            this.change = change;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot.Name);
            if (num == 1)
            {
                LoadFromSingleNode(xmlRoot.FirstChild);
            }
            else if (num > 1)
            {
                LoadMultipleNodes(xmlRoot);
            }
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
            {
                change = ParseHelper.FromString<float>(xmlText.InnerText);
            }
            else if (node is XmlElement element)
            {
                ParseXmlElement(element);
            }
        }

        private void LoadMultipleNodes(XmlNode xmlRoot)
        {
            foreach (object childNode in xmlRoot.ChildNodes)
            {
                ParseXmlElement(childNode as XmlElement);
            }
        }

        private void ParseXmlElement(XmlElement element)
        {
            if (element.Name == "change")
            {
                change = ParseHelper.FromString<float>(element.InnerText);
            }
        }
    }
}

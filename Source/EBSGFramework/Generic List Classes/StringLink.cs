using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class StringLink
    {
        public string text;

        public int num = 1;

        public StringLink()
        {
        }

        public StringLink(string text, int num)
        {
            this.text = text;
            this.num = num;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name != "li")
                text = xmlRoot.Name;

            switch (xmlRoot.ChildNodes.Count)
            {
                case 0:
                    num = 1;
                    break;
                case 1:
                    LoadFromSingleNode(xmlRoot.FirstChild);
                    break;
                default:
                    LoadMultipleNodes(xmlRoot);
                    break;
            }
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                num = ParseHelper.FromString<int>(xmlText.InnerText);
            else if (node is XmlElement element)
                ParseXmlElement(element);
        }

        private void LoadMultipleNodes(XmlNode xmlRoot)
        {
            foreach (var childNode in xmlRoot.ChildNodes)
                ParseXmlElement(childNode as XmlElement);
        }

        private void ParseXmlElement(XmlElement element)
        {
            if (element.Name == "num")
                num = ParseHelper.FromString<int>(element.InnerText);
            else if (element.Name == "text")
                text = element.InnerText;
        }
    }
}
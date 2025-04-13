using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class StatCheck
    {
        public StatDef stat;

        public float minStatValue = 0f;

        public float maxStatValue = 99999f;

        public FloatRange range = new FloatRange(0, 99999);

        public FloatRange defaultRange = new FloatRange(0, 99999);

        public StatCheck() { }

        public StatCheck(StatDef stat, FloatRange range)
        {
            this.stat = stat;
            this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.Name);

            if (num == 0)
                range = defaultRange;
            else if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                range = ParseHelper.FromString<FloatRange>(xmlText.InnerText);
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
            if (element.Name == "stat")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", element.InnerText);
            if (element.Name == "range")
                range = ParseHelper.FromString<FloatRange>(element.InnerText);
            else if (element.Name == "minStatValue")
                range.min = ParseHelper.FromString<float>(element.InnerText);
            else if (element.Name == "maxStatValue")
                range.max = ParseHelper.FromString<float>(element.InnerText);
        }
    }
}

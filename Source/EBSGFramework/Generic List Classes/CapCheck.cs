using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class CapCheck
    {
        public PawnCapacityDef capacity;

        public float minCapValue = 0f;

        public float maxCapValue = 9999f;

        public FloatRange range = new FloatRange(0, 9999);

        public FloatRange defaultRange = new FloatRange(0, 9999);

        public CapCheck() { }

        public CapCheck(PawnCapacityDef capacity, FloatRange range)
        {
            this.capacity = capacity;
            this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "capacity", xmlRoot.Name);

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
            if (element.Name == "capacity")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "capacity", element.InnerText);
            if (element.Name == "range")
                range = ParseHelper.FromString<FloatRange>(element.InnerText);
            else if (element.Name == "minCapValue")
                range.min = ParseHelper.FromString<float>(element.InnerText);
            else if (element.Name == "maxCapValue")
                range.max = ParseHelper.FromString<float>(element.InnerText);
        }
    }
}

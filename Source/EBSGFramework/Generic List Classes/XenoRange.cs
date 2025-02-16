using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class XenoRange
    {
        public XenotypeDef xenotype;

        public FloatRange range;

        public XenoRange() { }

        public XenoRange(XenotypeDef xenotype, FloatRange range)
        {
            this.xenotype = xenotype;
            this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "xenotype", xmlRoot.Name);

            if (num == 1)
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
            if (element.Name == "range")
                range = ParseHelper.FromString<FloatRange>(element.InnerText);
        }
    }
}

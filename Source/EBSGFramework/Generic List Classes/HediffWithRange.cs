using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class HediffWithRange
    {
        public HediffDef hediff;

        public FloatRange range;

        public static FloatRange defaultRange = new FloatRange(0f, 99999f);

        public HediffWithRange() { }

        public HediffWithRange(HediffDef hediff, FloatRange range)
        {
            this.hediff = hediff;
            this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "hediff", xmlRoot.Name);

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
            if (element.Name == "range")
                range = ParseHelper.FromString<FloatRange>(element.InnerText);
        }

    }
}

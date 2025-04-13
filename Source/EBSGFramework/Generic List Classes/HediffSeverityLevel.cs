using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class HediffSeverityLevel
    {
        public HediffDef hediff;

        public float minSeverity = 0f;

        public float maxSeverity = 99999f;

        public FloatRange range = new FloatRange(0, 99999);

        public FloatRange defaultRange = new FloatRange(0, 99999);

        public HediffSeverityLevel() { }

        public HediffSeverityLevel(HediffDef hediff, FloatRange range)
        {
            this.hediff = hediff;
            this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
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
            if (element.Name == "hediff")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "hediff", element.InnerText);
            if (element.Name == "range")
                range = ParseHelper.FromString<FloatRange>(element.InnerText);
            else if (element.Name == "minSeverity")
                range.min = ParseHelper.FromString<float>(element.InnerText);
            else if (element.Name == "maxSeverity")
                range.max = ParseHelper.FromString<float>(element.InnerText);
        }
    }
}

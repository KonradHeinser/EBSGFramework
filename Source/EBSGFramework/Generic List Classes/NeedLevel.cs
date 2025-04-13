using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class NeedLevel
    {
        public NeedDef need;

        public float minNeedLevel = 0f;

        public float maxNeedLevel = 1f;

        public FloatRange range = FloatRange.ZeroToOne;

        public FloatRange defaultRange = FloatRange.ZeroToOne;

        public NeedLevel() { }

        public NeedLevel(NeedDef need, FloatRange range)
        {
            this.need = need;
            this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "need", xmlRoot.Name);

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
            if (element.Name == "need")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "need", element.InnerText);
            if (element.Name == "range")
                range = ParseHelper.FromString<FloatRange>(element.InnerText);
            else if (element.Name == "minNeedLevel")
                range.min = ParseHelper.FromString<float>(element.InnerText);
            else if (element.Name == "maxNeedLevel")
                range.max = ParseHelper.FromString<float>(element.InnerText);
        }
    }
}
